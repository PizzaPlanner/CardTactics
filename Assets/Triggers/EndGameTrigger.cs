using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EndGameTrigger : Trigger
{
    public override IEnumerator Execute(Board board, Map map, Unit ___, PlayerInfo __)
    {
        board.CloseBoard();
        yield return null;
    }
}