using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/Immortal", fileName = "Immortal")]
public class LooseConditionImmortal : LooseCondition
{
    public override bool Check(GameSettings settings, PlayerInfo player) => false;

}