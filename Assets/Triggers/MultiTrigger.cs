using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [SerializeField] Trigger[] SubTriggers;

    public override IEnumerator Execute(Unit unit)
    {
        foreach (var trigger in SubTriggers)
            RoomNetworkManager.Playables.Enqueue(new TriggerContext(unit, trigger));
        yield return null;
    }
}