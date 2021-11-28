using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class GameStateMachine : NetworkBehaviour
{
    public enum Stage
    {
        Setup,
        StartTurn,
        Plan,
        Play,
        EndTurn,
        StartRound,
        Events,
        End

    }

    void Start()
    {
        CurrentStage = Stage.Setup;
        playableUnits = new Queue<Unit>();
        WaitingForAction = true;
    }

    public Stage CurrentStage { get => currentStage; set { currentStage = value; Resume(); } }

    [SyncVar] Stage currentStage;
    [SyncVar] public Unit CurrentUnit;
    Queue<Unit> playableUnits;


    public int Round => round;
    [SyncVar] int round;
    [SyncVar] int cardsPlayedForCurrentUnit;

    [SyncVar] public bool WaitingForAction;

    [Server]
    public void StartGame()
    {
        CurrentStage = Stage.Events;
        Resume();
    }

    Board board;

    [Server]
    void OnPlay()
    {
        var next = board.Playables.Next();
        foreach (var player in FindObjectsOfType<PlayerController>())
            player.Playables.Enqueue(next);

        IEnumerator PlayAndResume()
        {
            Debug.Log($"[Playable] Play {next.Name} for {next.Unit.Name}");
            yield return next.Play(false, true);

            if (!board.Playables.Empty)
                CurrentStage = Stage.Play;
            else
                CurrentStage = Stage.Events;
        }
        WaitingForAction = true;
        StartCoroutine(PlayAndResume());
    }

    [Server]
    void OnEvents()
    {
        var map = FindObjectOfType<Map>();
        map.AddTriggers(board);
        board.CheckDeath(map);

        if (!board.Playables.Empty)
            CurrentStage = Stage.Play;
        else if (CurrentUnit != null)
            CurrentStage = Stage.Plan;
        else if (playableUnits.Count() > 0)
            CurrentStage = Stage.StartTurn;
        else
            CurrentStage = Stage.StartRound;
        // else if (Map.Teams.Sum(x => x.RegisteredPlayers.Count(y => y.IsAlive)) > 0)
        //     CurrentStage = Stage.StartRound;
        // else
        //     CurrentStage = Stage.End;
    }

    [Server]
    void OnEnd()
    {
        WaitingForAction = true;
        Debug.Log("Game ended.");
    }

    [Server]
    void OnStartRound()
    {
        playableUnits.Clear();
        var playables = new List<Unit>();
        foreach (var unit in FindObjectsOfType<Unit>())
            if (unit.TryGetComponent<IngameDeck>(out var deck))
            {
                deck.OnRoundEnd();
                unit.Resources.Refresh();
                playables.Add(unit);
                deck.OnRoundStart();
            }
        playables.Sort((x, y) => Unit.CompareSpeed(x, y));
        foreach (var item in playables)
        {
            playableUnits.Enqueue(item);
        }

        if (playableUnits.Count() > 0)
            CurrentStage = Stage.StartTurn;
        else
            CurrentStage = Stage.End;

        WaitingForAction = true;
        round++;
        Debug.Log("Sending RoudnStartMessage");
        NetworkServer.SendToAll(new RoundStartMessage { round = round });
        Resume();
    }

    [Server]
    void OnEndTurn()
    {
        // turnPanel.OnTurnEnd(CurrentUnit);
        Debug.Log($"[Board] End Turn of {CurrentUnit.Name}");
        if (CurrentUnit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnEnd();
        CurrentUnit = null;
        if (board.Playables.Empty)
            CurrentStage = Stage.Events;
        else
            CurrentStage = Stage.Play;
    }

    [Server]
    void OnStartTurn()
    {
        CurrentUnit = playableUnits.Dequeue();
        Debug.Log($"[Board] Start Turn of {CurrentUnit.Name}");
        cardsPlayedForCurrentUnit = 0;
        WaitingForAction = true;
        var netId = CurrentUnit.GetComponent<NetworkIdentity>();
        Debug.Assert(netId != null);
        Debug.Log("Sending TurnStartMessage");
        NetworkServer.SendToAll(new TurnStartMessage { netId = netId });
        Resume();
        if (CurrentUnit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnStart();
        if (board.Playables.Empty)
            CurrentStage = Stage.Plan;
        else
            CurrentStage = Stage.Play;
    }

    [Server]
    void OnPlan()
    {
        var player = CurrentUnit.Player;
        WaitingForAction = true;
        if (player.IsAI)
            board.AIPrepare(CurrentUnit);
        else
            board.PlayerPrepare(CurrentUnit);
    }

    [Server]
    public void Resume()
    {
        WaitingForAction = false;
    }

    public override void OnStartServer()
    {
        board = FindObjectOfType<Board>();
    }

    void Update()
    {
        if (!WaitingForAction && isServer)
        {
            Debug.Log("[Board] -- " + CurrentStage + " --");
            switch (CurrentStage)
            {
                case Stage.StartTurn:
                    OnStartTurn(); break;
                case Stage.Plan:
                    OnPlan(); break;
                case Stage.Play:
                    OnPlay(); break;
                case Stage.EndTurn:
                    OnEndTurn(); break;
                case Stage.StartRound:
                    OnStartRound(); break;
                case Stage.Events:
                    OnEvents(); break;
                case Stage.End:
                    OnEnd(); break;
            }
        }
    }

}