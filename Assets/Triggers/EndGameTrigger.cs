using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EndGameTrigger : Trigger
{
    protected override IEnumerator Execute(Map map, Tile tile)
    {
        var survivors = map.Players.Where(x => !x.LooseCondition.Check(map, x)).Select(x => x.Team).Distinct();
        if (survivors.Count() > 1)
            Debug.Log("Game ended in Tie");
        else
            foreach (var player in map.Players)
                Debug.Log($"End game for {player.Deck.Commander?.Name}: {(survivors.Contains(player.Team) ? "Victory" : "Loss")}");
        LoadingScreen.GoToMenu();
        yield return null;
    }
}