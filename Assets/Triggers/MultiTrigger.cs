using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [SerializeField] Trigger[] SubTriggers;

    protected override IEnumerator Execute(Map map, PlayerInfo player)
    {
        foreach (var trigger in SubTriggers)
            yield return trigger.Play(map, player);
    }
}