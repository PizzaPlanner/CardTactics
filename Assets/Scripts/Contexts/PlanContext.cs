using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlanContext : IPlayableContextWithUnit, IClientPlayable, IServerPlayable
{
    public bool Queueable => true;
    public string Name => $"Planning of {Unit.Name}";
    public Unit Unit { get; set; }

    public PlanContext(Unit unit) => Unit = unit;
    public PlanContext() { }

    public bool PlayOnServer()
    {
        if (Unit.Player.IsAI)
        {
            var deck = Unit.GetComponent<IngameDeck>();
            var rnd = Random.Range(0, deck.Playable.Count());
            Board.FindObjectOfType<Board>().SetPlayerReady(deck.Playable.ToArray()[rnd]);
        }
        return true;
    }

    public IEnumerator PlayOnClient()
    {
        var localOwner = Unit.hasAuthority;

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
