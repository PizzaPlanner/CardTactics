using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/Commander dead", fileName = "CommanderDead")]
public class LooseConditionCommanderDead : LooseCondition
{
    public override bool Check(Map map, PlayerInfo player)
    {
        return player.Deck.Commander == null;
    }
}