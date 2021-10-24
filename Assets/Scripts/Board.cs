using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{

    public enum Stage
    {
        Setup,
        Execute,
        Play,
        Plan,
        Events,
        End

    }

    [ShowInInspector]
    bool WaitingForAction;

    [ShowInInspector]
    public Stage CurrentStage { get; private set; }

    TurnPlate turnPanel;
    Map Map;

    [ShowInInspector]
    Queue<(PlayerInfo player, IPlayable playable)> queuedPlayables;

    [ShowInInspector]
    IEnumerable<PlayerInfo> RegisteredPlayers => Map.Players.Where(x => x.State == PlayerInfo.PlayerState.Active);

    List<PlayerInfo> readyPlayers;

    Queue<HumanPlayer> unassignedPlayers;

    // Start is called before the first frame update
    public void Initialize(Map mapInstance, HumanPlayer[] players)
    {
        CurrentStage = Stage.Setup;
        Map = mapInstance;
        unassignedPlayers = new Queue<HumanPlayer>();
        queuedPlayables = new Queue<(PlayerInfo, IPlayable)>();
        if (players != null)
            foreach (var player in players)
                unassignedPlayers.Enqueue(player);
        WaitingForAction = true;
    }

    public bool Register(PlayerInfo player)
    {
        if (player.State != PlayerInfo.PlayerState.Hidden) return false;

        player.Initialize(player.IsAI || !unassignedPlayers.TryDequeue(out var human) ? null : human);
        turnPanel.AddPlayer(player);
        player.State = PlayerInfo.PlayerState.Active;
        return true;
    }

    public void StartGame()
    {
        CurrentStage = Stage.Events;
        Resume();
    }

    void OnPlay()
    {
        if (queuedPlayables.Count == 0)
        {
            CurrentStage = Stage.Events;
        }
        else
        {
            var (player, playable) = queuedPlayables.Dequeue();
            IEnumerator PlayAndResume()
            {
                currentPlayable = playable;
                yield return playable.Play(Map, player);
                currentPlayable = null;
                Resume();
            }
            WaitingForAction = true;
            StartCoroutine(PlayAndResume());

            turnPanel.OnRoundStart();
        }

    }

    void OnEvents()
    {
        var triggers = Map.CheckTriggers(CheckDeath(Map));
        if (triggers.Count > 0)
        {
            foreach (var trigger in triggers)
                QueuePlayable(trigger.player, trigger.trigger, false);
        }
        foreach (var player in Map.GetSpawnablePlayers())
        {
            if (Register(player))
                QueuePlayable(player, player.Deck.Commander, true);
        }

        if (queuedPlayables.Count() > 0)
            CurrentStage = Stage.Play;
        else if (RegisteredPlayers.Count() > 0)
        {
            readyPlayers = new List<PlayerInfo>();
            turnPanel.OnPreparation();
            CurrentStage = Stage.Plan;
        }
        else
        {
            turnPanel.OnPreparation();
            CurrentStage = Stage.End;
        }
    }

    void OnEnd()
    {
        Debug.Log("No Player left! Game ended!");
        LoadingScreen.GoToMenu();
    }

    void OnPlan()
    {
        foreach (var player in RegisteredPlayers)
        {
            if (readyPlayers.Contains(player)) continue;
            player.Deck.Shuffle();
            if (player.IsAI)
                AIPrepare(player);
            else
            {
                WaitingForAction = true;
                PlayerPrepare(player);
            }
        }
        if (readyPlayers.Count == RegisteredPlayers.Count())
            CurrentStage = Stage.Execute;
    }

    void OnExecute()
    {
        var cardsPlay = new List<(PlayerInfo player, Card card)>();
        foreach (var player in RegisteredPlayers)
            foreach (var card in player.Deck.Queued)
            {
                cardsPlay.Add((player, card));
            }
        cardsPlay.Sort((x, y) =>
        {
            var deckComp = x.player.Deck.CurrentSpeed.CompareTo(y.player.Deck.CurrentSpeed);
            if (deckComp == 0)
                return x.card.Speed.CompareTo(y.card.Speed);
            else
                return deckComp;
        });

        foreach (var (player, card) in cardsPlay)
            QueuePlayable(player, card, false);

        CurrentStage = Stage.Play;
    }

    IPlayable currentPlayable;
    public void Resume()
    {
        WaitingForAction = false;
    }

    void Awake()
    {
        if (Map == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
        else
            turnPanel = GameObject.FindObjectOfType<TurnPlate>();
    }

    public void QueuePlayable(PlayerInfo player, IPlayable playable, bool priorizeNext)
    {
        if (playable is Card card)
        {
            if (card.State != Card.CardState.Queued)
            {
                if (card.State == Card.CardState.Unplayed)
                    card.SetSelectionState(true);
                else
                {
                    Debug.LogError($"Failed to Queue {card.Name}: State is {card.State}");
                    return;
                }
            }
            turnPanel.OnCardInQueue(card, player);
            Debug.Log($"Queued up {card.Name} for {player.Name}");
        }
        else
            Debug.Log($"Queued up {playable.GetType().Name} for {player.Name}");

        queuedPlayables.Enqueue((player, playable));
        if (priorizeNext)
            while (!queuedPlayables.Peek().Equals((player, playable)))
                queuedPlayables.Enqueue(queuedPlayables.Dequeue());
    }

    void Update()
    {
        if (!WaitingForAction)
        {
            Debug.Log("Now entering Stage " + CurrentStage);
            switch (CurrentStage)
            {
                case Stage.Execute:
                    OnExecute(); break;
                case Stage.Play:
                    OnPlay(); break;
                case Stage.Plan:
                    OnPlan(); break;
                case Stage.Events:
                    OnEvents(); break;
                case Stage.End:
                    OnEnd(); break;
            }
        }
        else if (CurrentStage == Stage.Play && !EventSystem.current.IsPointerOverGameObject() && currentPlayable is Card currentCard)
        {
            if (Input.GetMouseButtonDown(0) && Tile.Hovered != null)
            {
                if (Tile.Hovered.CurrentIndicator == TargetIndicator.Valid)
                    currentCard.SkillTarget = Tile.Hovered;
            }
            if (Input.GetMouseButtonDown(1))
            {
                currentCard.WantReset = true;
            }
        }
    }

    void AIPrepare(PlayerInfo aiPlayer)
    {
        while (aiPlayer.Deck.Playable.Count() > 0)
            aiPlayer.Deck.Playable.First().SetSelectionState(true);

        SetPlayerReady(aiPlayer);
    }

    void PlayerPrepare(PlayerInfo player)
    {
        if (player.Deck.Playable.Count() > 0)
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player, onConfirm: () => SetPlayerReady(player));
        else
            SetPlayerReady(player);
    }

    public void SetPlayerReady(PlayerInfo player)
    {
        if (!player.Deck.QueueIsValid)
        {
            Debug.LogWarning("Error with Queue. Ouch!");
            QueuePlayable(player, player.OnInvalidQueueTrigger, true);
        }

        readyPlayers.Add(player);
        Resume();
    }

    List<PlayerInfo> CheckDeath(Map map)
    {
        var deadPlayers = new List<PlayerInfo>();
        foreach (var player in RegisteredPlayers)
        {
            if (player.LooseCondition.Check(map, player))
            {
                deadPlayers.Add(player);
            }
        }
        foreach (var player in deadPlayers)
        {
            Debug.Log($"<color={player.Color}>{player.Deck.Commander.Name}</color> died!");
            turnPanel.RemovePlayer(player);
            player.State = PlayerInfo.PlayerState.Dead;
        }
        return deadPlayers;
    }
}
