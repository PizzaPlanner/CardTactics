using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public static class SelectionFilters
{
    public static SelectionFilter Self { get; } = new SelectionFilter { Unit = UnitFilter.Unit, Tile = TileFilter.Self };
    public static SelectionFilter AnyEnemy { get; } = new SelectionFilter { Unit = ~UnitFilter.Map, Tile = TileFilter.Enemy };
    public static SelectionFilter AnyUnit { get; } = new SelectionFilter { Unit = ~UnitFilter.Map };
    public static SelectionFilter Everything { get; } = new SelectionFilter { Tile = 0, Range = RangeFilter.Any };
    public static SelectionFilter Empty { get; } = new SelectionFilter { Tile = TileFilter.Empty, Range = RangeFilter.Any };
}

[System.Serializable]
public struct SelectionFilter
{
    [EnumToggleButtons]
    [GUIColor(0.8f, 0.5f, 0.5f)]
    [HideLabel]
    public TileFilter Tile;
    [EnumToggleButtons]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [HideLabel]
    public UnitFilter Unit;
    [EnumToggleButtons]
    [GUIColor(0.5f, 0.5f, 0.8f)]
    [HideLabel]
    public RangeFilter Range;
}

[System.Flags]
public enum TileFilter
{
    Empty = 1,
    Enemy = 2,
    Ally = 4,
    Obstacle = 8,
    Self = 16,
    All = int.MaxValue
}

[System.Flags]
public enum UnitFilter
{
    Unit = 1,
    Summon = 2,
    Illusion = 4,
    Map = 8,
    All = int.MaxValue
}

public enum RangeFilter
{
    Any = 0,
    Path = 1,
    Sight = 2,
    All = int.MaxValue
}


public class SkillExecuteContext : IPlayableContextWithUnit, IClientPlayable, IServerPlayable
{
    public ITilePattern SkillTarget { get; set; }
    public SkillInfo Target { get; set; }
    public Unit Unit { get; set; }
    public Tile OutTile, OriginTile, SourceTile;

    public bool Queueable => true;

    public string Name => Target.Skill.GetType().Name + " (Execute)";

    public IEnumerator PlayOnClient()
    {
        yield return Target.Skill.ExecuteClient(this);
    }

    public IEnumerator PlayOnServer()
    {
        yield return Target.Skill.ExecuteServer(this);
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteSkillInfo(Target);
        writer.WriteTile(OriginTile);
        writer.WriteTile(SourceTile);
        writer.WriteTile(OutTile);
        writer.WritePattern(SkillTarget);
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Target = reader.ReadSkillInfo();
        OriginTile = reader.ReadTile();
        SourceTile = reader.ReadTile();
        OutTile = reader.ReadTile();
        SkillTarget = reader.ReadPattern();
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }
}

public class SkillPlanContext : IPlayableContextWithUnit, IServerPlayable, IClientPlayable
{
    public struct CancelContextMessage : NetworkMessage
    {
        public static bool CancelCurrentContext;
    }

    public SkillInfo Target { get; set; }
    public Tile OutTile;
    public Tile SourceTile;
    public Tile OriginTile { get; private set; }
    public Progress PlayProgress;
    public ITilePattern SkillTarget { get; set; }
    public bool ManualSending => true;
    public Unit Unit { get; private set; }
    public NetworkConnection Owner => Unit.Owner.connectionToClient;
    IEnumerable<SkillPlanContext> previousContexts;
    public string Name => Target.Skill.GetType().Name + " (Prepare)";
    public bool Queueable => true;

    public SkillPlanContext(SkillInfo target, Tile sourceTile, IEnumerable<SkillPlanContext> previousContexts)
    {
        Target = target;
        OriginTile = sourceTile;
        Unit = sourceTile.Unit;
        this.previousContexts = previousContexts;
    }

    public SkillPlanContext(SkillInfo target, Unit unit, Tile sourceTile = null)
    {
        Target = target;
        OriginTile = sourceTile;
        Unit = unit;
        previousContexts = new SkillPlanContext[0];
    }

    public bool Preview(out IEnumerable<ITilePattern> validPatterns, out IEnumerable<ITilePattern> invalidPatterns)
    {
        if (Target.CanUse(OriginTile, new SkillPlanContext[0], out SourceTile)
            && Target.Skill.Activate(this, out validPatterns, out invalidPatterns))
            return true;

        validPatterns = new ITilePattern[0];
        invalidPatterns = new ITilePattern[0];
        return false;
    }

    public IEnumerator PlayOnClient()
    {
        if (Target.Skill.Activate(this, out var validPatterns, out var _))
            Map.Instance.ShowPatterns(validPatterns, Unit.hasAuthority);
        yield return new WaitUntil(() => CancelContextMessage.CancelCurrentContext);
        CancelContextMessage.CancelCurrentContext = false;
        yield return GameSettings.Instance.SkillPrepareDelay.Yield();
        Map.Instance.HideTileSelectionPreview();
    }

    public IEnumerator PlayOnServer()
    {
        PlayProgress = Progress.Failed;
        if (Target.CanUse(OriginTile, previousContexts, out SourceTile)
            && Target.Skill.Activate(this, out var validPatterns, out var _))
        {
            PlayProgress = Progress.Now;
            Debug.Log($"[Card] Use Skill {Target.Skill.GetType().Name}: <color=green>OK</color>");
            if (Target.Skill.AutoSelectOnOnePossibleTile && validPatterns.Count() == 1)
                SkillTarget = validPatterns.First();
            else if (Unit.Player.IsAI)
                SkillTarget = validPatterns.ToArray()[UnityEngine.Random.Range(0, validPatterns.Count())];
            else
            {
                Map.Instance.AssignPatterns(validPatterns,
                (pattern) => TargetIndicator.Valid,
                (itp) => SkillTarget = itp);
                Debug.Log("[Board] Waiting for remote Player to use Skill...");
            }

            NetworkServer.SendToAll(new PlayableMessage { context = this });

            yield return new WaitUntil(() => SkillTarget != null);

            NetworkServer.SendToAll(new CancelContextMessage());

            if (SkillTarget != null)
            {
                OutTile = Target.Skill.GetOutTile(this);
                Debug.Assert(OutTile != null);
                PlayProgress = Progress.Queued;
                RoomNetworkManager.Playables.InsertFirst(new SkillExecuteContext
                {
                    SkillTarget = SkillTarget,
                    Unit = Unit,
                    Target = Target,
                    OutTile = OutTile,
                    SourceTile = SourceTile,
                    OriginTile = OriginTile
                });
            }
            else
            {
                PlayProgress = Progress.Skipped;
            }
        }
        else
            Debug.Log($"[Card] Use Skill {Target.Skill.GetType().Name}: <color=red>Condition Failed</color>");
        Map.Instance.HideTileSelectionPreview();
    }

    public SkillPlanContext() { }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteSkillInfo(Target);
        writer.WriteTile(OriginTile);
        writer.WriteTile(SourceTile);
        writer.WriteTile(OutTile);
        writer.Write((byte)PlayProgress);
        writer.WriteNetworkBehaviour(Unit);
        writer.Write(previousContexts.Count());
        foreach (var item in previousContexts)
            writer.WritePlayable(item);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Target = reader.ReadSkillInfo();
        OriginTile = reader.ReadTile();
        SourceTile = reader.ReadTile();
        OutTile = reader.ReadTile();
        PlayProgress = (Progress)reader.ReadByte();
        Unit = reader.ReadNetworkBehaviour<Unit>();
        var arr = new SkillPlanContext[reader.ReadInt()];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = (SkillPlanContext)reader.ReadPlayable();
        previousContexts = arr;
        return this;
    }
}

public enum SkillChain
{
    _ = 0,
    A = 1,
    B = 2,
    C = 3,
    D = 4,
    E = 5,
    F = 6
}

public enum SkillTrigger
{
    Use,
    TurnStart,
    TurnEnd,
    RoundStart,
    RoundEnd
}

[Serializable]
public class SkillInfo : IPlayable
{
    [VerticalGroup("Skill")]
    [HideLabel]
    public SkillTrigger Trigger;

    [VerticalGroup("Skill")]
    [HorizontalGroup("Skill/Horizontal", 5)]
    [HideLabel]
    [SuffixLabel("->")]
    [LabelWidth(10)]
    public SkillChain SourceTileInput;

    [VerticalGroup("Skill")]
    [HorizontalGroup("Skill/Horizontal", 5)]
    [HideLabel]
    public SkillChain TileOutput;

    [VerticalGroup("Skill")]
    [HorizontalGroup("Skill/Horizontal")]
    [LabelText("If")]
    public SkillChain[] Conditions;

    [VerticalGroup("Skill")]
    [HideLabel]
    [SerializeReference]
    public ISkill Skill;


    Tile GetInputTile(Tile origin, IEnumerable<SkillPlanContext> previousContexts)
    {
        var previous = previousContexts;
        if (SourceTileInput == SkillChain._) return null;
        var wanted = SourceTileInput;
        var inputInfo = previousContexts.LastOrDefault(x => x.Target.TileOutput == wanted);
        if (wanted == SkillChain.A && inputInfo == null)
            return origin;
        return inputInfo?.OutTile;
    }

    public bool CanUse(Tile origin, IEnumerable<SkillPlanContext> previousContexts, out Tile inputTile)
    {
        inputTile = null;
        if (previousContexts.Any(x => x.Target == this))
            return false;
        if (Conditions != null)
            foreach (var condition in Conditions)
                if (previousContexts.LastOrDefault(x => x.Target.TileOutput == condition)?.PlayProgress != Progress.Queued) return false;
        if (SourceTileInput != SkillChain._ && (inputTile = GetInputTile(origin, previousContexts)) == null) return false;
        return true;
    }
}
public enum Progress
{
    Pending,
    Queued,
    Skipped,
    Failed,
    Now
}

public interface ISkill
{
    string Description { get; }
    bool IsOptional => false;
    bool AutoSelectOnOnePossibleTile { get; }

    bool Activate(SkillPlanContext ctx, out IEnumerable<ITilePattern> validPatterns, out IEnumerable<ITilePattern> invalidPatterns)
    {
        var patterns = CreatePatterns(ctx);
        validPatterns = patterns.Where(x => x.Tiles.Any() && x.Selectable);
        invalidPatterns = patterns.Where(x => x.Tiles.Any() && !x.Selectable);
        var hasAny = validPatterns.Any();
        return hasAny;
    }

    Tile GetOutTile(SkillPlanContext ctx) => ctx.SkillTarget.Tiles.First();
    IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx);
    IEnumerator ExecuteServer(SkillExecuteContext ctx) { yield return null; }
    IEnumerator ExecuteClient(SkillExecuteContext ctx) { yield return null; }
    void Serialize(NetworkWriter writer);

    ISkill Deserialize(NetworkReader reader);
}