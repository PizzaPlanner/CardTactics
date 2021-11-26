using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[System.Serializable]
public class SkillContext : IPlayableContext
{
    public SkillInfo Target { get; set; }
    public ITilePattern SkillTarget { get; private set; }
    public Tile OutTile;
    public Tile SourceTile;
    public Tile OriginTile { get; private set; }
    public CardContext Source { get; private set; }
    public bool WantSkillSkip { get; private set; }
    public bool WantComplete { get; private set; }
    public Skill.Progress PlayProgress;
    public Unit Unit { get; private set; }

    public string Name => Target.Skill.GetType().Name;
    public bool Queueable => true;

    public SkillContext(SkillInfo target, CardContext source, Tile sourceTile)
    {
        Target = target;
        Source = source;
        OriginTile = sourceTile;
        Unit = source.Unit;
    }

    public SkillContext(SkillInfo target, Unit unit, Tile sourceTile = null)
    {
        Target = target;
        Source = Board.FindObjectOfType<Board>().CreateFakeCardContext();
        OriginTile = sourceTile;
        Unit = unit;
    }

    public bool Preview(out IEnumerable<ITilePattern> validPatterns, out IEnumerable<ITilePattern> invalidPatterns)
    {
        if (Target.CanUse(OriginTile, Source, out SourceTile)
            && Target.Skill.Activate(this, out validPatterns, out invalidPatterns))
            return true;

        validPatterns = new ITilePattern[0];
        invalidPatterns = new ITilePattern[0];
        return false;
    }

    public IEnumerator Play()
    {
        var map = Source.Map;
        var player = Unit.Player;
        if (Target.CanUse(OriginTile, Source, out SourceTile)
            && Target.Skill.Activate(this, out var validPatterns, out var _))
        {
            Debug.Log($"[Card] Use Skill {Target.Skill.GetType().Name}: <color=green>OK</color>");
            if (Target.Skill.AutoSelectOnOnePossibleTile && validPatterns.Count() == 1)
                SkillTarget = validPatterns.First();
            else if (player.IsAI)
                SkillTarget = validPatterns.ToArray()[UnityEngine.Random.Range(0, validPatterns.Count())];
            else if (player.Identity.isLocalPlayer)
                map.ShowTileIndicators(validPatterns,
                (pattern) => TargetIndicator.Valid,
                (itp) => SkillTarget = itp);
            else
                Debug.Log("[Board] Waiting for remote Player to use Skill...");

            yield return new WaitUntil(() => SkillTarget != null || (WantSkillSkip && Target.Skill.IsOptional));

            if (SkillTarget != null)
            {
                yield return Target.Skill.OnPreview(this);
                if (OutTile == null) Debug.LogError("[Card] Did not assign OutTile!");
                PlayProgress = global::Skill.Progress.Queued;
                if (player.IsAI)
                    WantComplete = true;
            }
            else if (WantSkillSkip)
            {
                PlayProgress = global::Skill.Progress.Skipped;
            }
        }
        else
            Debug.Log($"[Card] Use Skill {Target.Skill.GetType().Name}: <color=red>Condition Failed</color>");
        map.HideTileSelectionPreview();
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

[Serializable]
public class SkillInfo : IPlayable
{

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

    [HideLabel]
    [VerticalGroup("Skill")]
    [SerializeReference]
    public Skill Skill;


    Tile GetInputTile(Tile origin, CardContext ctx)
    {
        var previous = ctx.PreviousContexts;
        if (SourceTileInput == SkillChain._) return null;
        var wanted = SourceTileInput;
        var inputInfo = ctx.PreviousContexts.LastOrDefault(x => x.Target.TileOutput == wanted);
        if (wanted == SkillChain.A && inputInfo == null)
            return origin;
        return inputInfo?.OutTile;
    }

    public bool CanUse(Tile origin, CardContext ctx, out Tile inputTile)
    {
        inputTile = null;
        if (ctx.PreviousContexts.Any(x => x.Target == this))
            return false;
        if (Conditions != null)
            foreach (var condition in Conditions)
                if (ctx.PreviousContexts.LastOrDefault(x => x.Target.TileOutput == condition)?.PlayProgress != Skill.Progress.Queued) return false;
        if (SourceTileInput != SkillChain._ && (inputTile = GetInputTile(origin, ctx)) == null) return false;
        return true;
    }
}

[System.Serializable]
public abstract class Skill
{
    public enum Progress
    {
        Pending,
        Queued,
        Skipped,
        Failed,
        Now
    }

    public Card Source { get; set; }
    public abstract string Description { get; }
    public bool IsOptional => SkippableWithInput;

    [SerializeField] bool SkippableWithInput;
    public abstract bool AutoSelectOnOnePossibleTile { get; }


    public bool Activate(SkillContext ctx, out IEnumerable<ITilePattern> validPatterns, out IEnumerable<ITilePattern> invalidPatterns)
    {
        validPatterns = OnActivate(ctx).Where(x => x.Tiles.Any() && x.Selectable);
        invalidPatterns = OnActivate(ctx).Where(x => x.Tiles.Any() && !x.Selectable);
        var hasAny = validPatterns.Any();
        ctx.PlayProgress = hasAny ? Progress.Now : Progress.Failed;
        return hasAny;
    }

    protected abstract IEnumerable<ITilePattern> OnActivate(SkillContext ctx);

    public abstract IEnumerator OnPreview(SkillContext ctx);

    public virtual List<Card> GetRelatedCards()
    {
        var list = new List<Card>();
        if (Source != null)
            list.Add(Source);
        return list;
    }
}