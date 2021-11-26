using System.Collections;
using UnityEngine;

[System.Serializable]
public class RevealAreaTrigger : Trigger
{
    public int Area;

    public override IEnumerator Execute(Board board, Map map, Unit _)
    {
        yield return GameSettings.Instance.RevealAreaPreDelay.Yield();
        map.RevealArea(Area);
        yield return GameSettings.Instance.RevealAreaPostDelay.Yield();
    }
}