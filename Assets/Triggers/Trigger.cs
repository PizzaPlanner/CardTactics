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
        this.unit = unit;
        this.Player = unit.Player;
    }

    Board board;
    Map map;
    Unit unit;
    public PlayerInfo Player { get; private set; }

    public string Name => Target.GetType().Name;
    public bool Queueable => unit != null && (Target.CanTriggerMultipleTimes || !Target.Executed);

    public IEnumerator Play()
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(board, map, unit, Player);
    }
}

[System.Serializable]
public abstract class Trigger : MonoBehaviour, IPlayable
{
    public bool ShouldTrigger(Map map, Tile tile) => (!Executed || CanTriggerMultipleTimes) && (tile == null || tile.Unit != null);

    [SerializeField] public bool CanTriggerMultipleTimes;

    public bool Executed;

    public abstract IEnumerator Execute(Board board, Map map, Unit unit, PlayerInfo player);
}