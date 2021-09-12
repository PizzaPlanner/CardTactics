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
    protected Passive TargetPassive => TargetTile.Unit?.Card.Passive;
    protected Passive SourcePassive => SourceTile.Unit?.Card.Passive;
    public bool IsOptional => SkippableWithInput || dependents.Count == 0;
    public bool IsPreviewed { get; private set; }

    protected GameSettings Settings { get; private set; }

    [SerializeField] bool SkippableWithInput;

    List<Skill> dependents;

    public void Init(GameSettings settings, Card card)
    {
        Settings = settings;
        dependents = new List<Skill>();
        for (int i = 0; i < card.Dependencies.Length; i++)
        {
            var dep = card.Skills[i - card.Dependencies[i]];
            if (dep == this && card.Skills[i] != this)
                dependents.Add(dep);
        }
    }

    public bool Activate(GameSettings.PlayerInfo player, Card card, Skill dependency, Skill condition)
    {
        if (condition != null && condition.TargetTile == null) return false;
        SourceTile = dependency?.TargetTile;
        if ((dependency?.SkippableWithInput ?? false) && SourceTile == null)
        {
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