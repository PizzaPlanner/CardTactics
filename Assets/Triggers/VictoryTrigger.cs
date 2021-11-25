using System.Collections;
using UnityEngine;

[System.Serializable]
public class VictoryTrigger : Trigger
{
    public override IEnumerator Execute(Board board, Map map, Unit _, PlayerInfo player)
    {
        Debug.Log("Victory!!");
        yield return GameSettings.Instance.EndGameDelay.Yield();
        LoadingScreen.GoToMenu();
    }
}