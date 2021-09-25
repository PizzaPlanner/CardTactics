using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueTrigger : Trigger
{
    [System.Serializable]
    class DialogueLine
    {
        public string Text;
        public int Speaker;
    }

    [SerializeField] DialogueLine[] Lines;

    public override IEnumerator Execute(Map map, Tile tile)
    {
        foreach (var line in Lines)
        {
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            yield return ui.Set(map.Players[line.Speaker].Deck.Commander.UnitReference, line.Text);
        }
    }
}