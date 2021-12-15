using System.Collections;
using UnityEngine;

[System.Serializable]
public class EndGameTrigger : Trigger
{
    public bool Victory;

    public override IEnumerator Execute(Unit unit)
    {
        if (Victory)
            Debug.Log("Victory!");
        else
            Debug.Log("Defeat...");
        yield return GameSettings.Instance.EndGameDelay.Yield();
        LoadingScreen.GoToMenu();
    }
}