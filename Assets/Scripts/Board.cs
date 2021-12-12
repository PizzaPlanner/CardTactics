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

    public PlayableQueue()
    {
        queue = new();
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
            if (context is IPlayableContextWithUnit ipcu)
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} for {ipcu.Unit.Name}", ipcu.Unit);
            else
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} (Server)");
            return;
        }
        if (context is IPlayableContextWithUnit ipcu2)
            Debug.Log($"[Playable] Queue {context.Name} for {ipcu2.Unit.Name}", ipcu2.Unit);
        else
            Debug.Log($"[Playable] Queue {context.Name}");

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

    public PlayableQueue Playables { get; } = new();

    [SerializeField]
    public Card OnNoCardsPlayed;


    void Start()
    {
        if (isClient)
            settings.Load();
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
            NetworkServer.SendToAll(new StartPlanningMessage { netId = unit.GetComponent<NetworkIdentity>() });
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
        if (deck.Playable.Count() > 0)
        {
            NetworkServer.SendToAll(new StartPlanningMessage { netId = unit.GetComponent<NetworkIdentity>() });
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
        if (card.Unit != FindObjectOfType<GameStateMachine>().CurrentUnit)
        {
            Debug.LogError($"[Board] Invalid ready message received by {card.Unit.Name}!");
            return;
        }
        Playables.Enqueue(card);
        FindObjectOfType<GameStateMachine>().NotifyCardPlayed();
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
}
