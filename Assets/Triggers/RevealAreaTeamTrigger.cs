using System.Collections;
using UnityEngine;

[System.Serializable]
public class RevealAreaTeamTrigger : TeamTrigger
{
    public int Area;

    public override IEnumerator Execute(Board board, Map map, Team team)
    {
        yield return GameSettings.Instance.RevealAreaPreDelay.Yield();
        map.RevealArea(Area);
        yield return GameSettings.Instance.RevealAreaPostDelay.Yield();
    }
}