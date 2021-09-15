using System.Collections;
using System.Security.Policy;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Marker")]
public class MarkerTrigger : Trigger
{
    public int Marker;
    public bool Revealed;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        if (Revealed)
            settings.Map.ShowMarker(Marker);
        else
            settings.Map.HideMarker(Marker);
        yield return new WaitForSeconds(1);
    }
}