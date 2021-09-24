using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueTrigger : Trigger
{
    [System.Serializable]
    class DialogueLine
    {
        public string Text;
        public Unit Speaker;
    }

    [SerializeField] DialogueLine[] Lines;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        foreach (var line in Lines)
        {
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            yield return ui.Set(line.Speaker ?? tile.Unit, line.Text);
        }
    }
}