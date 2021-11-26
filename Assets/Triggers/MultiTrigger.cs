using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [SerializeField] Trigger[] SubTriggers;

    public override IEnumerator Execute(Board board, Map map, Unit unit)
    {
        foreach (var trigger in SubTriggers)
            board.Playables.Enqueue(new TriggerContext(unit, map, board, trigger));
        yield return null;
    }
}