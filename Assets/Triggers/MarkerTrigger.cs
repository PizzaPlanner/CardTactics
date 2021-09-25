using System.Collections;
using System.Security.Policy;
using UnityEngine;

[System.Serializable]
public class MarkerTrigger : Trigger
{
    public int Marker;
    public bool Revealed;

    public override IEnumerator Execute(Map map, Tile tile)
    {
        if (Revealed)
            map.ShowMarker(Marker);
        else
            map.HideMarker(Marker);
        yield return new WaitForSeconds(1);
    }
}