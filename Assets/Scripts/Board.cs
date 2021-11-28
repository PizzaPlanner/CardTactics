using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayableQueue
{
    public struct PlayableWithInfo
    {
        public PlayerSlot Player;
        public Unit Unit;
        public IPlayable Playable;
    }

    bool isServer;

    public PlayableQueue(bool isServer)
    {
        queue = new();
        this.isServer = isServer;
    }

    public bool Empty => queue.Count == 0;

    readonly Queue<IPlayableContext> queue;

    public void Enqueue(IPlayableContext context) => Enqueue(context, false);
    public void InsertFirst(IPlayableContext context) => Enqueue(context, true);

    public IPlayableContext Next() => queue.Dequeue();

    void Enqueue(IPlayableContext context, bool priorizeNext)
    {
        if (!context.Queueable)
        {
            Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} for {context.Unit.Name}", context.Unit);
            return;
        }
        Debug.Log($"[Playable] Queue {context.Name} for {context.Unit.Name}", context.Unit);

        if (priorizeNext)
        {
            var old = queue.ToArray();
            queue.Clear();
            queue.Enqueue(context);
            foreach (var item in old)
                queue.Enqueue(item);
        }
        else
            queue.Enqueue(context);
    }
}

public class Board : NetworkBehaviour
{

    [SerializeField] GameSettings settings;
    [SerializeField] GameStateMachine stateMachine;

    TurnPanel turnPanel;
    Map Map;

    public PlayableQueue Playables { get; } = new(true);

    [SerializeField]
    Card OnNoCardsPlayed;

    public void Initialize(Map mapInstance)
    {
        settings.Load();
        Map = mapInstance;
    }

    void Awake()
    {
        if (Map == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
        else
            turnPanel = GameObject.FindObjectOfType<TurnPanel>();
    }

    void Update()
    {
        if (isClient && stateMachine.CurrentStage == GameStateMachine.Stage.Play && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && Tile.Hovered != null)
            {
                if (Tile.Hovered.CurrentIndicator == TargetIndicator.Valid)
                    Map.OnTileSelection?.Invoke(Tile.Hovered.PrimaryPattern);
            }
        }

        if (isClient && stateMachine.WaitingForAction)
            switch (stateMachine.CurrentStage)
            {
                case GameStateMachine.Stage.Plan:
                case GameStateMachine.Stage.Play:
                    DoControls();
                    break;
            }
    }

    void DoControls()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
            Camera.main.orthographicSize += scroll;
    }

    public void AIPrepare(Unit unit)
    {
        var deck = unit.GetComponent<IngameDeck>();
        if (deck == null)
        {
            SetPlayerDone();
            return;
        }

        if (deck.Playable.Count() > 0)
        {
            var rnd = Random.Range(0, deck.Playable.Count());
            SetPlayerReady(deck.Playable.ToArray()[rnd]);
        }
        else
            SetPlayerDone();
    }

    public void PlayerPrepare(Unit unit)
    {
        var deck = unit.GetComponent<IngameDeck>();
        if (deck == null)
        {
            SetPlayerDone();
            return;
        }

        if (deck.Playable.Count() > 0)
        {
            var netId = unit.GetComponent<NetworkIdentity>();
            Debug.Log("Sending OnStartPlanning with netId " + netId);
            Debug.Assert(netId != null);
            NetworkServer.SendToAll(new StartPlanningMessage { netId = netId });
        }
        else
            SetPlayerDone();
    }

    public void SetPlayerDone()
    {
        // if (cardsPlayedForCurrentUnit == 0)
        // {
        //     Debug.LogWarning($"{CurrentUnit.Name} ended without playing a Card!");
        //     var oncp = new CardContext(OnNoCardsPlayed, Map, CurrentUnit, CardState.Unplayed);
        //     Playables.InsertFirst(oncp);
        // }

        stateMachine.CurrentStage = GameStateMachine.Stage.EndTurn;
    }

    public void SetPlayerReady(CardContext card)
    {
        Playables.Enqueue(card);
        // cardsPlayedForCurrentUnit++;
        stateMachine.CurrentStage = GameStateMachine.Stage.Play;
    }

    [Server]
    public void CheckDeath(Map map)
    {
        foreach (var team in map.Teams)
        {
            foreach (var condition in team.Conditions.ToArray())
                if (condition.Check(map, team))
                {
                    team.Conditions.Remove(condition);
                    Playables.InsertFirst(new TeamTriggerContext(team, map, this, condition.Trigger));
                }
        }
    }

    internal CardContext CreateFakeCardContext()
    {
        return new CardContext(new ActionCard(), Map, null, CardState.Unplayed);
    }
}
