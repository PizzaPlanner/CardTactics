using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public static class SelectionFilters
{
    public static SelectionFilter Self { get; } = new SelectionFilter { Unit = UnitFilter.Commander, Tile = TileFilter.Self };
    public static SelectionFilter AnyEnemy { get; } = new SelectionFilter { Unit = ~UnitFilter.Environment, Tile = TileFilter.Enemy };
    public static SelectionFilter AnyUnit { get; } = new SelectionFilter { Unit = ~UnitFilter.Environment };
    public static SelectionFilter Empty { get; } = new SelectionFilter { Tile = TileFilter.Empty, Range = RangeFilter.Any };
}

[System.Serializable]
public struct SelectionFilter
{
    [EnumToggleButtons]
    [HideLabel]
    public TileFilter Tile;
    [EnumToggleButtons]
    [HideLabel]
    public UnitFilter Unit;
    [EnumToggleButtons]
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
    Self = 16
}

[System.Flags]
public enum UnitFilter
{
    Commander = 1,
    Summon = 2,
    Illusion = 4,
    Environment = 8
}

public enum RangeFilter
{
    Any = 0,
    Pathfinding = 1,
    SightUnblocked = 2
}

public interface ICancelable
{
    void OnCancel();
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

[System.Serializable]
public class SkillInfo
{
    [LabelText("If")]
    public SkillChain[] Conditions;

    [HorizontalGroup(5)]
    [HideLabel]
    [SuffixLabel("->")]
    [LabelWidth(10)]
    public SkillChain SourceTileInput;

    [HorizontalGroup(5)]
    [HideLabel]
    public SkillChain TileOutput;

    [HideLabel]
    [SerializeReference]
    public Skill Skill;


    public Tile GetInputTile(Tile origin, IEnumerable<SkillInfo> previousSkills)
    {
        if (SourceTileInput == SkillChain._) return null;
        var wanted = SourceTileInput;
        var inputInfo = previousSkills.Where(x => x.TileOutput == wanted).LastOrDefault();
        if (wanted == SkillChain.A && inputInfo == null)
            return origin;
        return inputInfo?.GetOutputTile();
    }

    public Tile GetOutputTile()
    {
        return Skill.PlayProgress == Skill.Progress.Queued ? Skill.OutTile : Skill.SourceTile;
    }

    public bool CanUse(Tile origin, IEnumerable<SkillInfo> previousSkills, out Tile inputTile)
    {
        inputTile = null;
        foreach (var condition in Conditions)
            if (previousSkills.Where(x => x.TileOutput == condition).LastOrDefault()?.Skill.PlayProgress != Skill.Progress.Queued) return false;
        if (SourceTileInput != SkillChain._ && (inputTile = GetInputTile(origin, previousSkills)) == null) return false;
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

    [PreviewField(32)]
    [AssetSelector(Paths = "Assets/3rdParty/64 flat icons/png/64px", FlattenTreeView = true)]
    public Sprite Icon;

    public Card Source { get; set; }
    public abstract string Description { get; }

    public ITilePattern TargetPattern { get; private set; }
    public Tile SourceTile { get; private set; }
    public Tile OutTile { get; protected set; }
    public bool IsOptional => SkippableWithInput;
    public bool IsPreviewed { get; private set; }

    public Tile[] ValidTiles { get; protected set; }

    [SerializeField] bool SkippableWithInput;
    protected abstract bool AutoSelectOnOnePossibleTile { get; }
    bool autoSelectIgnored;

    public Progress PlayProgress { get; private set; }

    public virtual void Prepare()
    {
        PlayProgress = Progress.Pending;
        SourceTile = null;
        OutTile = null;
        IsPreviewed = false;
    }

    public bool Activate(Map map, PlayerInfo player, Card card, Tile inputTile, out IEnumerable<ITilePattern> validPatterns)
    {
        PlayProgress = Progress.Failed;

        SourceTile = inputTile;

        validPatterns = OnActivate(map, player, card).Where(x => x.Tiles.Any() && x.Selectable);
        var hasAny = validPatterns.Any();
        PlayProgress = hasAny ? Progress.Now : Progress.Failed;
        if (!hasAny)
            return false;

        if (AutoSelectOnOnePossibleTile && !autoSelectIgnored && card != null)
        {
            card.SkillTarget = validPatterns.First();
        }
        else if (player.IsAI)
        {
            player.AIDecide(card, this, validPatterns);
        }

        return true;
    }

    protected abstract IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card);
    public IEnumerator Preview(ITilePattern target)
    {
        IsPreviewed = true;
        yield return OnPreview(target);
        PlayProgress = Progress.Queued;
        autoSelectIgnored = false;
    }

    public void Reset()
    {
        if (this is not ICancelable cancelable) return;
        if (IsPreviewed)
            cancelable.OnCancel();
        IsPreviewed = false;
        PlayProgress = Progress.Pending;
        autoSelectIgnored = true;
    }

    protected abstract IEnumerator OnPreview(ITilePattern targetPattern);
    protected abstract void OnUse();

    public void Use()
    {
        OnUse();
        IsPreviewed = false;
    }

    internal void Skip()
    {
        PlayProgress = Progress.Skipped;
        autoSelectIgnored = false;
        IsPreviewed = false;
    }

    public virtual List<Card> GetRelatedCards()
    {
        var list = new List<Card>();
        if (Source != null)
            list.Add(Source);
        return list;
    }
}