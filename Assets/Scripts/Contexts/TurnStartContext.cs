using System.Collections;
using Mirror;

public class TurnStartContext : IPlayableContextWithUnit, IClientPlayable, IServerPlayable
{
    public bool Queueable => true;
    public string Name => $"Turn of {Unit.Name}";
    public Unit Unit { get; set; }

    public TurnStartContext(Unit unit) => Unit = unit;
    public TurnStartContext() { }

    public IEnumerator PlayOnClient()
    {
        // StartCoroutine to run this in parallel with other animations
        var tp = TurnPanel.FindObjectOfType<TurnPanel>();
        tp.StartCoroutine(tp.OnTurnStart(Unit, null));
        yield return null;
    }

    public bool PlayOnServer()
    {
        if (Unit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnStart();
        if (RoomNetworkManager.Playables.Empty)
            GameStateMachine.CurrentStage = GameStateMachine.Stage.Plan;
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
