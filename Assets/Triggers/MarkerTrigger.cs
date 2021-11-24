using System.Collections;
using UnityEngine;

[System.Serializable]
public class MarkerTrigger : Trigger
{
    public int Marker;
    public bool Revealed;

    public override IEnumerator Execute(Board board, Map map, Unit _, PlayerInfo player)
    {
        if (Revealed)
            map.ShowMarker(Marker);
        else
            map.HideMarker(Marker);
        yield return new WaitForSeconds(1);
    }
}