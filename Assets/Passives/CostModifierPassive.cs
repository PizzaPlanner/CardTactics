using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Cost Modifier")]
public class CostModifierPassive : Passive
{
    public PlayCost Buff;

    public override string Description => $"While this card is active, you have {(Buff.Cost > 0 ? "+" : "")}{Buff.Cost} {Buff.Class.Name}";

    public override void OnRoundStart(Map map, PlayerInfo player)
    {

    }
}