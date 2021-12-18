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
        playableUnits = new Queue<Unit>();
        waitingForAction = true;
        CurrentStage = Stage.Events;
    }

    public static Stage CurrentStage { get => instance.currentStage; set { instance.currentStage = value; instance.Resume(); } }

    [SyncVar] Stage currentStage = Stage.Setup;
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
        waitingForAction = true;
        if (!RoomNetworkManager.Playables.TryGetNext(out var coroutine))
            CurrentStage = Stage.Events;
        else
        {
            StartCoroutine(PlayAndResume());
        }

        IEnumerator PlayAndResume()
        {
            yield return coroutine;
            if (!RoomNetworkManager.Playables.Empty)
                CurrentStage = Stage.Play;
            else
                CurrentStage = Stage.Events;
        }
    }

    [Server]
    void OnEvents()
    {
        var map = FindObjectOfType<Map>();
        map.AddTriggers(board);

        if (!RoomNetworkManager.Playables.Empty)
            CurrentStage = Stage.Play;
        else if (CurrentUnit != null && !CurrentUnit.IsDead)
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
        foreach (var unit in FindObjectsOfType<Unit>().Where(x => !x.IsDead))
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

        round++;
        RoomNetworkManager.Playables.Enqueue(new RoundStartContext { Round = round });
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
            var oncp = new CardContext(board.OnNoCardsPlayed, CurrentUnit, CardState.Unplayed);
            RoomNetworkManager.Playables.InsertFirst(oncp);
        }
        CurrentUnit = null;
        if (RoomNetworkManager.Playables.Empty)
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
        if (CurrentUnit.IsDead)
        {
            CurrentStage = Stage.Events;
            return;
        }

        Debug.Log($"[Board] Start Turn of {CurrentUnit.Name}");
        cardsPlayedForCurrentUnit = 0;
        RoomNetworkManager.Playables.Enqueue(new TurnStartContext { Unit = CurrentUnit });
        if (CurrentUnit.TryGetComponent<IngameDeck>(out var deck))
            deck.OnTurnStart();
        if (RoomNetworkManager.Playables.Empty)
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
        if (!waitingForAction)
            return;
        IEnumerator waitForPlayers()
        {
            yield return new WaitUntil(() => NetworkServer.connections.Values.All(x => x.isReady));
            waitingForAction = false;
        }
        StartCoroutine(waitForPlayers());
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