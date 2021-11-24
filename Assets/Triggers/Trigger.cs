using System.Collections;
using UnityEngine;

public interface IPlayable
{
}

public class TriggerContext : IPlayableContext
{
    public Trigger Target { get; private set; }
    public TriggerContext(PlayerInfo player, Unit unit, Map map, Board board, Trigger trigger)
    {
        Target = trigger;
        board = this.board;
        map = this.map;
        unit = this.unit;
        player = this.player;
    }

    Board board;
    Map map;
    Unit unit;
    PlayerInfo player;

    public IEnumerator Play()
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(board, map, unit, player);
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