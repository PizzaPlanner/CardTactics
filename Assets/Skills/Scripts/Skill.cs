using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelectionFilters
{
    public static SelectionFilter Self { get; } = new SelectionFilter { Unit = UnitFilter.Commander, Tile = TileFilter.Self };
    public static SelectionFilter AnyEnemy { get; } = new SelectionFilter { Unit = ~UnitFilter.Environment, Tile = TileFilter.Enemy };
    public static SelectionFilter Empty { get; } = new SelectionFilter { Tile = TileFilter.Empty, Range = RangeFilter.Any };
}

[System.Serializable]
public struct SelectionFilter
{
    public TileFilter Tile;
    public UnitFilter Unit;
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

public abstract class Skill : ScriptableObject
{
    public enum Progress
    {
        Pending,
        Queued,
        Skipped,
        Failed,
        Now
    }

    public string Name;
    public Sprite Icon;

    [NonSerialized] public Card Source;
    public abstract string Description { get; }

    protected Tile TargetTile { get; private set; }
    public Tile SourceTile { get; private set; }
    public bool IsOptional => SkippableWithInput;
    public bool IsPreviewed { get; private set; }

    public Tile[] ValidTiles { get; protected set; }

    [SerializeField] bool SkippableWithInput;
    protected abstract bool AutoSelectOnOnePossibleTile { get; }
    bool autoSelectIgnored;

    public Progress PlayProgress { get; private set; }

    public bool Activate(Map map, PlayerInfo player, Card card, Skill dependency, Skill condition)
    {
        PlayProgress = Progress.Failed;
        if (condition != null && condition.PlayProgress != Progress.Queued) return false;
        if (dependency != null)
        {
            if (dependency.PlayProgress != Progress.Queued && !dependency.SkippableWithInput)
                return false;
            if (dependency.PlayProgress == Progress.Queued)
                SourceTile = dependency.TargetTile;
            else
                SourceTile = dependency.SourceTile;
        }
        var validTiles = OnActivate(map, player, card);
        if (validTiles.Length == 1 && AutoSelectOnOnePossibleTile && !autoSelectIgnored && card != null)
        {
            card.SkillTarget = validTiles[0];
        }
        else if (validTiles.Length > 0 && player.IsAI)
        {
            player.AIDecide(card, this, validTiles);
        }

        PlayProgress = validTiles.Length > 0 ? Progress.Now : Progress.Failed;
        return validTiles.Length > 0;
    }

    protected abstract Tile[] OnActivate(Map map, PlayerInfo player, Card card);
    public IEnumerator Preview(Tile target)
    {
        TargetTile = target;
        IsPreviewed = true;
        yield return OnPreview();
        PlayProgress = Progress.Queued;
        autoSelectIgnored = false;
    }

    public void Reset()
    {
        if (this is not ICancelable cancelable) return;
        if (IsPreviewed)
            cancelable.OnCancel();
        IsPreviewed = false;
        TargetTile = null;
        PlayProgress = Progress.Pending;
        autoSelectIgnored = true;
    }

    protected abstract IEnumerator OnPreview();
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