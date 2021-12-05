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
        instance = this;
        CurrentStage = Stage.Setup;
        playableUnits = new Queue<Unit>();
        waitingForAction = true;
        Debug.Log("Hey! Game start! " + isClient + "," + isServer);
        CurrentStage = Stage.Events;
        Resume();
    }

    public static Stage CurrentStage { get => instance.currentStage; set { instance.currentStage = value; instance.Resume(); } }

    [SyncVar] Stage currentStage;
    [SyncVar] public Unit CurrentUnit;
    Queue<Unit> playableUnits;

    static GameStateMachine instance;

    public int Round => round;
    [SyncVar] int round;
    [SyncVar] int cardsPlayedForCurrentUnit;

    public static bool WaitingForAction => instance.waitingForAction;

    [SyncVar] public bool waitingForAction;

    Board board;

    [Server]
    void OnPlay()
    {
        var next = board.Playables.Next();
        foreach (var player in FindObjectsOfType<PlayerController>())
            player.Playables.Enqueue(next);

        IEnumerator PlayAndResume()
        {
            Debug.Log($"[Playable] Play {next.Name} for {next.Unit.Name} (Server)");
            yield return next.PlayOnServer();

            if (!board.Playables.Empty)
                CurrentStage = Stage.Play;
            else
                CurrentStage = Stage.Events;
        }
        waitingForAction = true;
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
        else if (FindObjectsOfType<Unit>().Count(x => !x.IsDead) > 0)
            CurrentStage = Stage.StartRound;
        else
            CurrentStage = Stage.End;
    }

    [Server]
    void OnEnd()
    {
        waitingForAction = true;
        Debug.Log("Game ended.");
    }

    [Server]
    void OnStartRound()
    {
        playableUnits.Clear();
        var unitsToPlay = new List<Unit>();
        foreach (var unit in FindObjectsOfType<Unit>())
            if (unit.TryGetComponent<IngameDeck>(out var deck))
            {
                deck.OnRoundEnd();
                unit.Resources.Refresh();
                unitsToPlay.Add(unit);
                deck.OnRoundStart();
            }
        unitsToPlay.Sort((x, y) => Unit.CompareSpeed(x, y));
        foreach (var item in unitsToPlay)
        {
            playableUnits.Enqueue(item);
        }

        if (playableUnits.Count() > 0)
            CurrentStage = Stage.StartTurn;
        else
            CurrentStage = Stage.End;

        waitingForAction = true;
        round++;
        Debug.Log("Sending RoudnStartMessage");
        NetworkServer.SendToAll(new RoundStartMessage { round = round });
        Resume();
    }

    [Server]
    void OnEndTurn()
    {
        Debug.Log($"[Board] End Turn of {CurrentUnit.Name}");
        if (CurrentUnit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnEnd();
        if (cardsPlayedForCurrentUnit == 0)
        {
            var board = FindObjectOfType<Board>();
            Debug.LogWarning($"[Board] {CurrentUnit.Name} ended without playing a Card!");
            var oncp = new CardContext(board.OnNoCardsPlayed, FindObjectOfType<Map>(), CurrentUnit, CardState.Unplayed);
            board.Playables.InsertFirst(oncp);
        }
        CurrentUnit = null;
        if (board.Playables.Empty)
            CurrentStage = Stage.Events;
        else
            CurrentStage = Stage.Play;
    }

    [Server]
    public void NotifyCardPlayed() => cardsPlayedForCurrentUnit++;

    [Server]
    void OnStartTurn()
    {
        CurrentUnit = playableUnits.Dequeue();
        Debug.Log($"[Board] Start Turn of {CurrentUnit.Name}");
        cardsPlayedForCurrentUnit = 0;
        waitingForAction = true;
        var netId = CurrentUnit.GetComponent<NetworkIdentity>();
        Debug.Assert(netId != null);
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
        waitingForAction = true;
        if (CurrentUnit.Player.IsAI)
            board.AIPlan(CurrentUnit);
        else
            board.PlayerPlan(CurrentUnit);
    }

    [Server]
    public void Resume()
    {
        waitingForAction = false;
    }

    public override void OnStartServer()
    {
        board = FindObjectOfType<Board>();
    }

    void Update()
    {
        if (!waitingForAction && isServer)
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