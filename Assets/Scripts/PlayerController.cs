using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public List<Unit> ControlledUnits;
    public static PlayerController Local;

    [SyncVar] public Deck Deck;
    [SerializeField] GameSettings settings;

    [Command(requiresAuthority = true)]
    public void SelectDeck(Deck deck)
    {
        Deck = Instantiate(deck);
    }

    public override void OnStartLocalPlayer()
    {
        Local = this;
        settings.Load();
    }

    [Command]
    public void SetPlayerReady(int cardId)
    {
        var unit = FindObjectOfType<GameStateMachine>().CurrentUnit;
        FindObjectOfType<Board>().SetPlayerReady(unit.GetComponent<IngameDeck>().Contexts[cardId]);
    }

    [Command]
    public void EndTurn()
    {
        FindObjectOfType<Board>().SetPlayerDone();
    }
}