using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Dialogue")]
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
            GameObject.FindObjectOfType<Board>().Paused = true;
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            yield return ui.Set(line.Speaker ?? tile.Unit, line.Text);
        }
    }
}