using System.Collections;
using System.Security.Policy;
using UnityEngine;

[System.Serializable]
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