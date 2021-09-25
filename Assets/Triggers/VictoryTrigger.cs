using System.Collections;
using UnityEngine;

[System.Serializable]
public class VictoryTrigger : Trigger
{
    public override IEnumerator Execute(Map map, Tile tile)
    {
        Debug.Log("Victory!!");
        yield return new WaitForSeconds(1);
        LoadingScreen.GoToMenu();
    }
}