using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class IngameDeck : NetworkBehaviour
{
    [ShowInInspector]
    public List<CardContext> Contexts;

    // [Server]
    public void OnRoundStart()
    {
        foreach (var card in Contexts)
            card.Target.OnRoundStart(card);
    }

    // [Server]
    public void OnRoundEnd()
    {
        foreach (var card in Contexts)
            card.Target.OnRoundEnd(card);
    }

    // [Server]
    public void OnTurnStart()
    {
        foreach (var card in Contexts)
            card.Target.OnTurnStart(card);
    }

    // [Server]
    public void OnTurnEnd()
    {
        foreach (var card in Contexts)
            card.Target.OnTurnEnd(card);
    }

    public IEnumerable<CardContext> All => Contexts;
    public IEnumerable<CardContext> Unplayed => Contexts.Where(x => x.State == CardState.Unplayed);
    public IEnumerable<CardContext> Playable => Unplayed.Where(x => x.Target is ActionCard acard && x.Unit.Resources.CanAfford(acard.Cost));
}
