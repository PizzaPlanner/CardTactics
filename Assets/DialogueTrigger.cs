using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
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

    public override void Execute(GameSettings settings, Tile tile)
    {
        Play(tile.Unit, 0);
    }

    public void Play(Unit unit, int line)
    {
        if (Lines.Length > line)
        {
            GameObject.FindObjectOfType<Board>().Paused = true;
            var ui = GameObject.FindObjectOfType<DialogueUI>(true);
            ui.Set(Lines[line].Speaker ?? unit, Lines[line].Text, () => Play(unit, line + 1));
        }
        else
        {
            GameObject.FindObjectOfType<Board>().Paused = false;
            GameObject.FindObjectOfType<Board>().TurnPanel.Next();
        }
    }
}