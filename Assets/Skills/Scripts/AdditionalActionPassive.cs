using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


[CreateAssetMenu(menuName = "Passives/Additional Action")]
public class AdditionalActionPassive : Passive
{
    public Skill Skill;

    public override void OnActivate(GameSettings settings, GameSettings.PlayerInfo player)
    {
        foreach (var card in player.Deck.Cards)
        {
            if (card is not ActionCard acard) continue;
            if (acard.Skills[acard.Skills.Length - 1] is not AttackSkill askl || askl.MaxRange != 1) continue;

            acard.ExtendWith(Skill, Source, acard.Skills.Length, 2, 1);
        }
    }
}