using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Passives/Movement Modifier")]
public class MoveModifierPassive : Passive
{
    List<MoveSkill> affected = new List<MoveSkill>();

    public RangeType RangeFilter;
    public int Modifier;

    public override void OnRoundStart(GameSettings settings, PlayerInfo player)
    {
        foreach (var card in player.Deck.Cards)
        {
            if (card is not ActionCard acard || acard.Skills.Length == 0) continue;
            foreach (var skill in card.Skills)
            {
                if (skill is MoveSkill mskill && !affected.Contains(mskill) && mskill.RangeType == RangeFilter)
                {
                    affected.Add(mskill);
                    mskill.Range += Modifier;
                    mskill.Name += $"<color=green>+{Modifier}</color>";
                }
            }
        }
    }
}