using System;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum TargetType
{
    Empty = 1,
    Enemy = 2,
    Ally = 4,
    Obstacle = 8
}

public enum RangeType
{
    Pathfinding,
    Any,
    SightUnblocked
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

    protected Tile TargetTile { get; private set; }
    public Tile SourceTile { get; private set; }
    public bool IsOptional => SkippableWithInput;
    public bool IsPreviewed { get; private set; }

    protected GameSettings Settings { get; private set; }

    [SerializeField] bool SkippableWithInput;

    public Progress PlayProgress;

    public void Init(GameSettings settings)
    {
        Settings = settings;
    }

    public bool Activate(GameSettings.PlayerInfo player, Card card, Skill dependency, Skill condition)
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
        var result = OnActivate(player, card);
        PlayProgress = result ? Progress.Now : Progress.Failed;
        return result;
    }

    protected abstract bool OnActivate(GameSettings.PlayerInfo player, Card card);
    public void Preview(Tile target)
    {
        TargetTile = target;
        IsPreviewed = true;
        OnPreview();
        PlayProgress = Progress.Queued;
    }

    public void Reset()
    {
        if (IsPreviewed)
            OnCancel();
        IsPreviewed = false;
        TargetTile = null;
        PlayProgress = Progress.Pending;
    }

    protected abstract void OnPreview();
    public abstract void OnUse();
    protected abstract void OnCancel();

    internal void Skip()
    {
        PlayProgress = Progress.Skipped;
    }
}