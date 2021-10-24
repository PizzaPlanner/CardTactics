using System.Collections;
using UnityEngine;

[System.Serializable]
public class VictoryTrigger : Trigger
{
    protected override IEnumerator Execute(Board board, Map map, PlayerInfo player)
    {
        Debug.Log("Victory!!");
        yield return new WaitForSeconds(1);
        LoadingScreen.GoToMenu();
    }
}