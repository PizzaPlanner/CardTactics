using System.Collections;
using UnityEngine;

[System.Serializable]
public class MarkerTrigger : Trigger
{
    public int Marker;
    public bool Revealed;

    public override IEnumerator Execute(Unit _)
    {
        var map = FindObjectOfType<Map>();
        if (Revealed)
            map.ShowMarker(Marker);
        else
            map.HideMarker(Marker);
        yield return GameSettings.Instance.MarkerDelayTime.Yield();
    }
}