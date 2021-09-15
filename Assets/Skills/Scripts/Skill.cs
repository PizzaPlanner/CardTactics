using System;
using System.Collections;
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

    public Tile[] ValidTiles { get; protected set; }

    protected GameSettings Settings { get; private set; }

    [SerializeField] bool SkippableWithInput;
    [SerializeField] bool AutoSelectOnOnePossibleTile;
    bool autoSelectIgnored;

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
        var validTiles = OnActivate(player, card);
        if (validTiles.Length == 1 && AutoSelectOnOnePossibleTile && !autoSelectIgnored && card != null)
        {
            card.SkillTarget = validTiles[0];
        }

        PlayProgress = validTiles.Length > 0 ? Progress.Now : Progress.Failed;
        return validTiles.Length > 0;
    }

    protected abstract Tile[] OnActivate(GameSettings.PlayerInfo player, Card card);
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
        if (IsPreviewed)
            OnCancel();
        IsPreviewed = false;
        TargetTile = null;
        PlayProgress = Progress.Pending;
        autoSelectIgnored = true;
    }

    protected abstract IEnumerator OnPreview();
    public abstract void OnUse();
    protected abstract void OnCancel();

    internal void Skip()
    {
        PlayProgress = Progress.Skipped;
        autoSelectIgnored = false;
    }
}