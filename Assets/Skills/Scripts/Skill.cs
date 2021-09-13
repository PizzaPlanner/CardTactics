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
    public string Name;
    public Sprite Icon;

    public Card Source;

    protected Tile TargetTile { get; private set; }
    public Tile SourceTile { get; private set; }
    public bool IsOptional => SkippableWithInput;
    public bool IsPreviewed { get; private set; }

    protected GameSettings Settings { get; private set; }

    [SerializeField] bool SkippableWithInput;

    public void Init(GameSettings settings)
    {
        Settings = settings;
    }

    public bool Activate(GameSettings.PlayerInfo player, Card card, Skill dependency, Skill condition)
    {
        if (condition?.IsPreviewed == false) return false;
        if (dependency != null)
        {
            if (!dependency.IsPreviewed && !dependency.SkippableWithInput)
                return false;
            if (dependency.IsPreviewed)
                SourceTile = dependency.TargetTile;
            else
                SourceTile = dependency.SourceTile;
        }
        return OnActivate(player, card);
    }

    protected abstract bool OnActivate(GameSettings.PlayerInfo player, Card card);
    public void Preview(Tile target)
    {
        TargetTile = target;
        IsPreviewed = true;
        OnPreview();
    }

    public void Reset()
    {
        if (IsPreviewed)
            OnCancel();
        IsPreviewed = false;
        TargetTile = null;
    }

    protected abstract void OnPreview();
    public abstract void OnUse();
    protected abstract void OnCancel();
}