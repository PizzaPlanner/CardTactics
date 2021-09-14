using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/Immortal", fileName = "Immortal")]
public class LooseConditionImmortal : LooseCondition
{
    public override bool Check(GameSettings settings, GameSettings.PlayerInfo player) => false;

}