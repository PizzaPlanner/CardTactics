using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Multiple")]
public class MultiTrigger : Trigger
{
    public Trigger[] Triggers;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        foreach (var trigger in Triggers)
            yield return trigger.Execute(settings, tile);
    }
}