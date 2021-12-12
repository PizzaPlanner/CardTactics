using System.Collections;
using System.Linq;
using Mirror;
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
    }

    Board board;
    Map map;
    Team team;

    public string Name => Target.GetType().Name;
    public bool Queueable => team != null && (Target.CanTriggerMultipleTimes || !Target.Executed);

    public IEnumerator PlayOnServer()
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(board, map, team);
    }

    public IEnumerator PlayOnClient()
    {
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(board);
        writer.Write(map);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        board = reader.Read<Board>();
        map = reader.Read<Map>();
        return this;
    }
}

[System.Serializable]
public abstract class TeamTrigger : MonoBehaviour, IPlayable
{
    [SerializeField] public bool CanTriggerMultipleTimes;

    public bool Executed;
    public abstract IEnumerator Execute(Board board, Map map, Team team);
}