using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : NetworkBehaviour
{
    [SerializeField]
    public Card OnNoCardsPlayed;

    [Server]
    public void SetPlayerDone()
    {
        GameStateMachine.CurrentStage = GameStateMachine.Stage.EndTurn;
    }

    [Server]
    public void SetPlayerReady(CardContext card)
    {
        if (card.Unit != FindObjectOfType<GameStateMachine>().CurrentUnit || card.Unit.IsDead)
        {
            Debug.LogError($"[Board] Invalid ready message received by {card.Unit.Name}!");
            SetPlayerDone();
            return;
        }
        RoomNetworkManager.Playables.Enqueue(card);
        FindObjectOfType<GameStateMachine>().NotifyCardPlayed();
        GameStateMachine.CurrentStage = GameStateMachine.Stage.Play;
    }
}
