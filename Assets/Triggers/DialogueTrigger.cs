using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DialogueTrigger : Trigger
{
    [System.Serializable]
    class DialogueLine
    {
        public string Text;
    }

    [SerializeField] DialogueLine[] Lines;

    public override IEnumerator Execute(Board board, Map map, Unit unit)
    {
        foreach (var line in Lines)
        {
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            yield return ui.Set(unit, line.Text);
        }
    }
}