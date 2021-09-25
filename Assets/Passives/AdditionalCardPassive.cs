using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Passives/Gain Card")]
public class AdditionalCardPassive : Passive
{
    public GainCardSkill Skill;
    public bool OneCardPerPassive;
    public Card.CardState InitialState;

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.AddRange(Skill.GetRelatedCards());
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
        player.Deck.ExtendWith(Skill.CardPrefab, Source, Card.CardState.Skipped);
    }
}