using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Multiple")]
public class MultiTrigger : Trigger
{
    public Trigger[] Triggers;

    public override void Execute(GameSettings settings, Tile tile)
    {
        foreach (var trigger in Triggers)
            trigger.Execute(settings, tile);
    }
}