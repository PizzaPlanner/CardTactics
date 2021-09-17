using System;
using System.Collections;
using UnityEngine;

public abstract class Trigger : ScriptableObject
{
    public virtual bool ShouldTrigger(GameSettings settings, Tile tile) => tile.Unit != null;

    public abstract IEnumerator Execute(GameSettings settings, Tile tile);
}