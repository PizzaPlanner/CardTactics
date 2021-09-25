using System.Collections;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [SerializeReference] Trigger[] SubTriggers;

    public override IEnumerator Execute(Map map, Tile tile)
    {
        foreach (var trigger in SubTriggers)
            yield return trigger.Execute(map, tile);
    }
}