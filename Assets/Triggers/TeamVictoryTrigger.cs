using System.Collections;
using UnityEngine;

[System.Serializable]
public class TeamVictoryTrigger : TeamTrigger
{
    public override IEnumerator Execute(Board board, Map map, Team team)
    {
        Debug.Log("Victory! " + team);
        yield return GameSettings.Instance.EndGameDelay.Yield();
        foreach (var player in team.RegisteredPlayers)
            if (!player.IsAI)
                Lobby.Current.Award($"Victory:{map.name}", player);
        board.CloseBoard();
    }
}
