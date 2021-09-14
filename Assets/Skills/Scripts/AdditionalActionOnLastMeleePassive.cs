using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


[CreateAssetMenu(menuName = "Passives/Additional Action")]
public class AdditionalActionOnLastMeleePassive : Passive
{
    public Skill Skill;
    public int Dependency;
    public int Condition;

    public override void OnRoundStart(GameSettings settings, GameSettings.PlayerInfo player)
    {
        foreach (var card in player.Deck.Cards)
        {
            if (card is not ActionCard acard || acard.Skills.Length == 0) continue;
            if (acard.Skills[acard.Skills.Length - 1] is not AttackSkill askl || askl.MaxRange != 1) continue;
            foreach (var skill in acard.Skills)
                if (skill.Source == this) continue;

            acard.ExtendWith(Skill, Source, acard.Skills.Length, Dependency, Condition);
        }
    }
}