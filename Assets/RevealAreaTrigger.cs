using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Reveal Area")]
public class RevealAreaTrigger : Trigger
{
    public int Area;

    public override void Execute(GameSettings settings, Tile tile)
    {
        settings.Map.RevealArea(Area);
    }
}