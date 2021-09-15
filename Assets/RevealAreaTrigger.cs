using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Reveal Area")]
public class RevealAreaTrigger : Trigger
{
    public int Area;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        yield return new WaitForSeconds(0.1f);
        settings.Map.RevealArea(Area);
        yield return new WaitForSeconds(0.5f);
    }
}