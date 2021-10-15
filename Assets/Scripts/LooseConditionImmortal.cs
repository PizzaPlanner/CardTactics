using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/Immortal", fileName = "Immortal")]
public class LooseConditionImmortal : LooseCondition
{
    public override bool Check(Map map, PlayerInfo player) => false;

}