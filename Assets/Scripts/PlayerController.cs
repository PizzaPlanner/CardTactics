using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public List<Unit> ControlledUnits;
    public static PlayerController Local;

    [SerializeField] GameSettings settings;


    static PlayerController[] lobby = new PlayerController[4];
    public static PlayerController FromLobby(int id) => lobby[id];
    public int LobbyId => lobby.ToList().IndexOf(this);
    public override void OnStartServer()
    {
        for (int i = 0; i < lobby.Length; i++)
            if (lobby[i] == null)
                lobby[i] = this;
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
        FindObjectOfType<Board>().SetPlayerReady(unit.GetComponent<IngameDeck>()[cardId]);
    }

    [Command]
    public void EndTurn()
    {
        FindObjectOfType<Board>().SetPlayerDone();
    }
}