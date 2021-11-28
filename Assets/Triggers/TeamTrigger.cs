using System.Collections;
using System.Linq;
using UnityEngine;

public class TeamTriggerContext : IPlayableContext
{
    public TeamTrigger Target { get; private set; }
    public TeamTriggerContext(Team team, Map map, Board board, TeamTrigger trigger)
    {
        Target = trigger;
        this.board = board;
        this.map = map;
        this.team = team;
        Unit = null; //team.RegisteredPlayers.First().Unit;
    }

    Board board;
    Map map;
    Team team;
    public Unit Unit { get; private set; }

    public string Name => Target.GetType().Name;
    public bool Queueable => team != null && (Target.CanTriggerMultipleTimes || !Target.Executed);

    public IEnumerator Play(bool isClient, bool isServer)
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(board, map, team);
    }
}

[System.Serializable]
public abstract class TeamTrigger : MonoBehaviour, IPlayable
{
    [SerializeField] public bool CanTriggerMultipleTimes;

    public bool Executed;
    public abstract IEnumerator Execute(Board board, Map map, Team team);
}