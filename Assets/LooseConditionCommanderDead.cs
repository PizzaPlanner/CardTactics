using UnityEngine;

[CreateAssetMenu(menuName = "Loose Conditions/Commander dead", fileName = "CommanderDead")]
public class LooseConditionCommanderDead : LooseCondition
{
    public override bool Check(GameSettings settings, GameSettings.PlayerInfo player)
    {
        return player.Deck.Commander.State == Card.CardState.Destroyed;
    }

}