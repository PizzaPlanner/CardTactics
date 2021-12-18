using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class IngameDeck : NetworkBehaviour
{
    [ShowInInspector]
    public List<CardContext> Contexts;

    [Server]
    public void OnRoundStart()
    {

    }

    [Server]
    public void OnRoundEnd()
    {
        foreach (var card in Contexts)
            if (card.State == CardState.Played || card.State == CardState.Shuffle)
                card.State = CardState.TurnStart;
    }

    [Server]
    public void OnTurnStart()
    {
        foreach (var card in Contexts)
        {
            if (card.State == CardState.TurnStart)
            {
                RoomNetworkManager.Playables.Enqueue(card);
            }
        }
    }

    [Server]
    public void OnTurnEnd()
    {
        foreach (var card in Contexts)
        {
            if (card.State == CardState.Unplayed)
            {
                card.State = CardState.TurnEnd;
                RoomNetworkManager.Playables.Enqueue(card);
            }
        }
    }

    public IEnumerable<CardContext> All => Contexts;
    public IEnumerable<CardContext> Unplayed => Contexts.Where(x => x.Target.Playable && (x.State != CardState.Played && x.State != CardState.Destroyed));
    public IEnumerable<CardContext> Played => Contexts.Where(x => x.Target.Playable && x.State == CardState.Played);
    public IEnumerable<CardContext> Unplayable => Contexts.Where(x => !x.Target.Playable && (x.State != CardState.Played && x.State != CardState.Destroyed));
    public IEnumerable<CardContext> Playable => Unplayed.Where(x => x.Target.Playable && x.Queueable && x.Target is ActionCard acard && x.Unit.Resources.CanAfford(acard.Cost));

    public int IndexOf(CardContext ctx) => Contexts.IndexOf(ctx);
}
