using UnityEngine;


[CreateAssetMenu(menuName = "Passives/Gain Card")]
public class AdditionalCardPassive : Passive
{
    public GainCardSkill Skill;
    public bool OneCardPerPassive;
    public Card.CardState InitialState;

    public override void OnRoundStart(GameSettings settings, GameSettings.PlayerInfo player)
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