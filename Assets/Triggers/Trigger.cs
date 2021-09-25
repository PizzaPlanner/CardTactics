using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public abstract class Trigger
{
    public virtual bool ShouldTrigger(Map map, Tile tile) => tile.Unit != null;

    public abstract IEnumerator Execute(Map map, Tile tile);
}