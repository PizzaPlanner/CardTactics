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
        public int Speaker;
    }

    [SerializeField] DialogueLine[] Lines;

    public override IEnumerator Execute(Board board, Map map, Unit _, PlayerInfo player)
    {
        foreach (var line in Lines)
        {
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            yield return ui.Set(map.Players.ToArray()[line.Speaker].Deck.Commander, line.Text);
        }
    }
}