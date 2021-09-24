using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public abstract class Trigger
{
    public virtual bool ShouldTrigger(GameSettings settings, Tile tile) => tile.Unit != null;

    public abstract IEnumerator Execute(GameSettings settings, Tile tile);
}