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

    Map Map;

    public PlayableQueue Playables { get; } = new(true);

    [SerializeField]
    public Card OnNoCardsPlayed;

    public void Initialize(Map mapInstance)
    {
        settings.Load();
        Map = mapInstance;
    }

    void Awake()
    {
        if (Map == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
    }

    [Server]
    public void AIPlan(Unit unit)
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

    [Server]
    public void PlayerPlan(Unit unit)
    {
        var deck = unit.GetComponent<IngameDeck>();
        if (deck == null)
        {
            SetPlayerDone();
            return;
        }
        Debug.Log($"[Board] Deck can play {deck.Playable.Count()} cards");
        if (deck.Playable.Count() > 0)
        {
            var netId = unit.GetComponent<NetworkIdentity>();
            NetworkServer.SendToAll(new StartPlanningMessage { netId = netId });
        }
        else
            SetPlayerDone();
    }

    [Server]
    public void SetPlayerDone()
    {
        GameStateMachine.CurrentStage = GameStateMachine.Stage.EndTurn;
    }

    [Server]
    public void SetPlayerReady(CardContext card)
    {
        Playables.Enqueue(card);
        stateMachine.NotifyCardPlayed();
        GameStateMachine.CurrentStage = GameStateMachine.Stage.Play;
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
