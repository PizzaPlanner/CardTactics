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

    protected Tile PreviewTile { get; private set; }
    public Tile OriginTile { get; private set; }
    public bool IsOptional => SkippableWithInput || dependents.Count == 0;
    public bool IsPreviewed { get; private set; }

    [SerializeField] bool SkippableWithInput;

    List<Skill> dependents;

    public void Init(Card card)
    {
        dependents = new List<Skill>();
        for (int i = 0; i < card.Dependencies.Length; i++)
        {
            var dep = card.Skills[i - card.Dependencies[i]];
            if (dep == this && card.Skills[i] != this)
                dependents.Add(dep);
        }
    }

    public bool Activate(int player, Card card, Skill dependency)
    {
        OriginTile = dependency?.PreviewTile;
        if ((dependency?.SkippableWithInput ?? false) && OriginTile == null)
        {
            OriginTile = dependency.OriginTile;
        }
        return OnActivate(player, card);
    }

    protected abstract bool OnActivate(int player, Card card);
    public void Preview(Tile target)
    {
        PreviewTile = target;
        IsPreviewed = true;
        OnPreview();
    }

    public void Reset()
    {
        if (IsPreviewed)
            OnCancel();
        IsPreviewed = false;
        PreviewTile = null;
    }

    protected abstract void OnPreview();
    public abstract void OnUse();
    protected abstract void OnCancel();
}