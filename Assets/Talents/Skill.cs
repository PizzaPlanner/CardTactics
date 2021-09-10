using System;
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

    public bool Activate(int player, Card card, Tile selection)
    {
        OriginTile = selection;
        return OnActivate(player, card, selection);
    }

    protected abstract bool OnActivate(int player, Card card, Tile selection);
    public void Preview(Tile target)
    {
        PreviewTile = target;
        OnPreview();
    }

    public Tile Reset()
    {
        var ot = OriginTile;
        OnCancel();
        PreviewTile = null;
        OriginTile = null;
        return ot;
    }

    protected abstract void OnPreview();
    public abstract void OnUse();
    protected abstract void OnCancel();
}