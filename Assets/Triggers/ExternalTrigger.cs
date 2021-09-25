using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ExternalTrigger : MultiTrigger
{
    [NonSerialized] public bool ConditionMet = false;

    public override bool ShouldTrigger(Map map, Tile tile)
    {
        return ConditionMet;
    }

    public override IEnumerator Execute(Map map, Tile tile)
    {
        ConditionMet = true;
        yield return base.Execute(map, tile);
    }
}
