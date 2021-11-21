using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/All Units dead", fileName = "UnitsDead")]
public class LooseConditionAllUnitsDead : LooseCondition
{
    public override bool Check(Map map, PlayerInfo player)
    {
        return map.GetPlayerTiles(player).Length == 0;
    }
}