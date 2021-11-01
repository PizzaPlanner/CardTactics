using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MultiTrigger : Trigger
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [SerializeField] Trigger[] SubTriggers;

    protected override IEnumerator Execute(Board board, Map map, Unit unit, PlayerInfo player)
    {
        foreach (var trigger in SubTriggers)
            board.Playables.Enqueue(player, unit, trigger);
        yield return null;
    }
}