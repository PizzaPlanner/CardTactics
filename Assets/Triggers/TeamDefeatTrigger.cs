using System.Collections;
using UnityEngine;

[System.Serializable]
public class TeamDefeatTrigger : TeamTrigger
{
    public override IEnumerator Execute(Board board, Map map, Team team)
    {
        Debug.Log("Defeated! " + team);
        yield return GameSettings.Instance.EndGameDelay.Yield();
    }
}