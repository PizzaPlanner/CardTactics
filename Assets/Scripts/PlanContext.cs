using System.Collections;
using Mirror;
using UnityEngine;

public class PlanContext : IPlayableContextWithUnit, IClientPlayable
{
    public bool Queueable => true;
    public string Name => $"Planning of {Unit.Name}";
    public Unit Unit { get; set; }

    public PlanContext(Unit unit) => Unit = unit;
    public PlanContext() { }

    public IEnumerator PlayOnClient()
    {
        var localOwner = Unit.Owner == PlayerController.Local;

        GameObject.FindObjectOfType<PlanningPanel>(true).Show(Unit,
        onPlay: localOwner ? ((cardId) => PlayerController.Local.SetPlayerReady(cardId)) : null,
        onEndTurn: localOwner ? (() => PlayerController.Local.EndTurn()) : null);

        yield return GameSettings.Instance.PlanningPanelPopupTime.Yield();
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
