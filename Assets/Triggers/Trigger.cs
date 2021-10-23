using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public abstract class Trigger : MonoBehaviour
{
    public bool ShouldTrigger(Map map, Tile tile) => (!Executed || CanTriggerMultipleTimes) && (tile == null || tile.Unit != null);

    [SerializeField] protected bool CanTriggerMultipleTimes;

    public bool Executed { get; private set; }

    public IEnumerator Run(Map map, Tile tile)
    {
        if (Executed && !CanTriggerMultipleTimes)
            yield break;
        Executed = true;
        yield return Execute(map, tile);
    }

    protected abstract IEnumerator Execute(Map map, Tile tile);
}