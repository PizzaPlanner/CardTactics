using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Gain Card")]
public class AdditionalCardPassive : Passive
{
    public Card CardPrefab;
    public bool OneCardPerPassive;
    public Card.CardState InitialState;

    public override string Description => $"At the start of each turn, gain {CardPrefab.Name}{(OneCardPerPassive ? ", if you don't already have one." : ".")}";

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.Add(CardPrefab);
        list.AddRange(CardPrefab.GetRelatedCards());
        return list;
    }

    public override void OnRoundStart(Map map, PlayerInfo player)
    {
        if (OneCardPerPassive)
        {
            foreach (var card in player.Deck.Cards)
            {
                if (card.Source == Source && card.State != Card.CardState.Destroyed) return;
            }
        }
        player.Deck.ExtendWith(CardPrefab, Source, InitialState);
    }
}