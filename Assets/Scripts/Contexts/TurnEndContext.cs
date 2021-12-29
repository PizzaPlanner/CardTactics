using System.Collections;
using Mirror;

public class TurnEndContext : IPlayableContextWithUnit, IClientPlayable, IServerPlayable
{
    public bool Queueable => true;
    public string Name => $"End Turn of {Unit.Name}";
    public Unit Unit { get; set; }

    public TurnEndContext(Unit unit) => Unit = unit;
    public TurnEndContext() { }

    public IEnumerator PlayOnClient()
    {
        var tp = TurnPanel.FindObjectOfType<TurnPanel>();
        tp.OnTurnEnd(Unit);
        yield return null;
    }
    public bool PlayOnServer()
    {
        if (Unit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnEnd();
        if (RoomNetworkManager.Playables.Empty)
            GameStateMachine.CurrentStage = GameStateMachine.Stage.Events;
        else
            GameStateMachine.CurrentStage = GameStateMachine.Stage.Play;
        return true;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }

}
