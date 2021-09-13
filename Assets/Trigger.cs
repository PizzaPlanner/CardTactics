using System;
using UnityEngine;

public abstract class Trigger : ScriptableObject
{
    public bool ShouldTrigger(Tile tile) => tile.Unit != null;

    public abstract void Execute(GameSettings settings, Tile tile);
}