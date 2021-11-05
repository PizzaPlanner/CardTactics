using System.Linq;
using UnityEngine;


[System.Serializable]
public class AdditionalActionOnLastMeleePassive : Passive
{
    public Skill Skill;
    public int Dependency;
    public int Condition;

    public override string Description => $"Whenever the last action of a card is a melee attack, you may {Skill.Description} afterwards.";

    public override void OnRoundStart(Map map, PlayerInfo player)
    {
        foreach (var card in player.Deck.All)
        {
            if (card is not ActionCard acard || acard.Skills.Count() == 0) continue;
            if (acard.Skills.Last() is not AttackSkill askl || askl.MaxRange != 1) continue;
            foreach (var skill in acard.Skills)
                if (skill.Source == this.Source) continue;

            // acard.ExtendWith(Skill, Source, acard.Skills.Length, Dependency, Condition);
        }
    }
}