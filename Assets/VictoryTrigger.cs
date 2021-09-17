using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Victory")]
public class VictoryTrigger : Trigger
{
    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        Debug.Log("Victory!!");
        yield return new WaitForSeconds(1);
        LoadingScreen.GoToMenu();
    }
}