using System.Collections;
using UnityEngine;

[System.Serializable]
public class RevealAreaTrigger : Trigger
{
    public int Area;

    protected override IEnumerator Execute(Map map, Tile tile)
    {
        yield return new WaitForSeconds(0.1f);
        map.RevealArea(Area);
        yield return new WaitForSeconds(0.5f);
    }
}