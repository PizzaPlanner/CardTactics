using System;
using System.Collections;
using UnityEngine;

public abstract class Trigger : ScriptableObject
{
    public bool ShouldTrigger(Tile tile) => tile.Unit != null;

    public abstract IEnumerator Execute(GameSettings settings, Tile tile);
}