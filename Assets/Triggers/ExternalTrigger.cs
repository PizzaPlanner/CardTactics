using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ExternalTrigger : MultiTrigger
{
    [NonSerialized] public bool ConditionMet = false;

    public override bool ShouldTrigger(GameSettings settings, Tile tile)
    {
        return ConditionMet;
    }

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        ConditionMet = true;
        yield return base.Execute(settings, tile);
    }
}
