using System;
using System.Collections;
using UnityEngine;

public interface IPlayable
{
    IEnumerator Play(Map map, PlayerInfo player);
}

[System.Serializable]
public abstract class Trigger : MonoBehaviour, IPlayable
{
    public bool ShouldTrigger(Map map, Tile tile) => (!Executed || CanTriggerMultipleTimes) && (tile == null || tile.Unit != null);

    [SerializeField] protected bool CanTriggerMultipleTimes;

    public bool Executed { get; private set; }

    public IEnumerator Play(Map map, PlayerInfo player)
    {
        if (Executed && !CanTriggerMultipleTimes)
            yield break;
        Executed = true;
        yield return Execute(map, player);
    }

    protected abstract IEnumerator Execute(Map map, PlayerInfo player);
}