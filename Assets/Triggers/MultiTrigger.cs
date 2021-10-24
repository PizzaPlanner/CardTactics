using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [SerializeField] Trigger[] SubTriggers;

    protected override IEnumerator Execute(Board board, Map map, PlayerInfo player)
    {
        foreach (var trigger in SubTriggers)
            board.QueuePlayable(player, trigger, false);
        yield return null;
    }
}