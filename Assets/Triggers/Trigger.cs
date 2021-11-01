using System.Collections;
using UnityEngine;

public interface IPlayable
{
    IEnumerator Play(Board board, Map map, Unit unit, PlayerInfo player);
}

[System.Serializable]
public abstract class Trigger : MonoBehaviour, IPlayable
{
    public bool ShouldTrigger(Map map, Tile tile) => (!Executed || CanTriggerMultipleTimes) && (tile == null || tile.Unit != null);

    [SerializeField] protected bool CanTriggerMultipleTimes;

    public bool Executed { get; private set; }

    public IEnumerator Play(Board board, Map map, Unit unit, PlayerInfo player)
    {
        if (Executed && !CanTriggerMultipleTimes)
            yield break;
        Executed = true;
        yield return Execute(board, map, unit, player);
    }

    protected abstract IEnumerator Execute(Board board, Map map, Unit unit, PlayerInfo player);
}