using System.Collections;
using UnityEngine;

[System.Serializable]
public class RevealAreaTrigger : Trigger
{
    public int Area;

    public override IEnumerator Execute(Unit _)
    {
        yield return GameSettings.Instance.RevealAreaPreDelay.Yield();
        Map.Instance.RevealArea(Area);
        yield return GameSettings.Instance.RevealAreaPostDelay.Yield();
    }
}