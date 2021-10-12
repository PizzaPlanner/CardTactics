using System.Collections;
using UnityEngine;

[System.Serializable]
public class RegisterPlayerTrigger : Trigger
{
    public int Player;

    public override IEnumerator Execute(Map map, Tile tile)
    {
        GameObject.FindObjectOfType<Board>(true).Register(map.Players[Player]);
        yield return map.Players[Player].Deck.Commander?.Play(map, map.Players[Player]);
    }
}