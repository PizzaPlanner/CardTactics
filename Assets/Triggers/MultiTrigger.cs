using System.Collections;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [SerializeReference] Trigger[] SubTriggers;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        foreach (var trigger in SubTriggers)
            yield return trigger.Execute(settings, tile);
    }
}