using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdditionalCardPassive : Passive
{
    public Card CardPrefab;
    public bool OneCardPerPassive;
    public Card.CardState InitialState;

    public override string Description => $"At the start of each turn, gain {CardPrefab.Name}{(OneCardPerPassive ? ", if you don't already have one." : ".")}";

    protected override TargetType Target => TargetType.Player;

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.Add(CardPrefab);
        list.AddRange(CardPrefab.GetRelatedCards());
        return list;
    }

    public override void OnRoundStart()
    {
        if (OneCardPerPassive)
        {
            foreach (var card in TargetPlayer.Deck.All)
            {
                if (card.Source == Source && card.State != Card.CardState.Destroyed) return;
            }
        }
        TargetPlayer.Deck.ExtendWith(CardPrefab, Source, InitialState);
    }
}