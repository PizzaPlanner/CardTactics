using System.Collections;
using UnityEngine;

public interface IPlayable
{
}

public class TriggerContext : IPlayableContext
{
    public Trigger Target { get; private set; }
    public TriggerContext(Unit unit, Map map, Board board, Trigger trigger)
    {
        Target = trigger;
        this.board = board;
        this.map = map;
        this.Unit = unit;
    }

    Board board;
    Map map;
    public Unit Unit { get; private set; }

    public string Name => Target.GetType().Name;
    public bool Queueable => Unit != null && (Target.CanTriggerMultipleTimes || !Target.Executed);

    public IEnumerator Play()
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(board, map, Unit);
    }
}

[System.Serializable]
public abstract class Trigger : MonoBehaviour, IPlayable
{
    public bool ShouldTrigger(Map map, Tile tile) => (!Executed || CanTriggerMultipleTimes) && (tile == null || tile.Unit != null);

    [SerializeField] public bool CanTriggerMultipleTimes;

    public bool Executed;

    public abstract IEnumerator Execute(Board board, Map map, Unit unit);
}