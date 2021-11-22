using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/All Units dead", fileName = "UnitsDead")]
public class LooseConditionAllUnitsDead : LooseCondition
{
    public override bool Check(Map map, Team team)
    {
        return map.Players.Count(x => x.Team == team && x.IsAlive) == 0;
    }
}