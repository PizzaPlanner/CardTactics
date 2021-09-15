using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Register Player")]
public class RegisterPlayerTrigger : Trigger
{
    public int Player;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        GameObject.FindObjectOfType<TurnPlate>(true).Register(settings.Players[Player]);
        yield return settings.Players[Player].Deck.Commander?.Play(settings, settings.Players[Player]);
    }
}