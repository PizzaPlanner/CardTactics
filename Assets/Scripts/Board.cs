using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{

    public enum Stage
    {
        Setup,
        PreparePlan,
        Plan,
        Play,
        StartTurn,
        Events,
        End

    }

    [ShowInInspector]
    bool WaitingForAction;

    [ShowInInspector]
    public Stage CurrentStage { get; private set; }

    TurnPanel turnPanel;
    Map Map;

    [ShowInInspector]
    Queue<(PlayerInfo player, IPlayable playable)> queuedPlayables;

    [ShowInInspector]
    IEnumerable<PlayerInfo> RegisteredPlayers => Map.Players.Where(x => x.State == PlayerInfo.PlayerState.Active);

    Queue<HumanPlayer> unassignedPlayers;

    // Start is called before the first frame update
    public void Initialize(Map mapInstance, HumanPlayer[] players)
    {
        CurrentStage = Stage.Setup;
        unassignedPlayers = new Queue<HumanPlayer>();
        queuedPlayables = new Queue<(PlayerInfo, IPlayable)>();
        playableUnits = new Queue<Unit>();
        if (players != null)
            foreach (var player in players)
                unassignedPlayers.Enqueue(player);
        Map = mapInstance;
        Map.Load(this);
        WaitingForAction = true;
    }

    public bool Register(PlayerInfo player)
    {
        if (player.State != PlayerInfo.PlayerState.Hidden) return false;

        player.Initialize(player.IsAI || !unassignedPlayers.TryDequeue(out var human) ? null : human);
        player.State = PlayerInfo.PlayerState.Active;

        QueuePlayable(player, player.Deck.Commander, true);
        return true;
    }

    public void StartGame()
    {
        CurrentStage = Stage.Events;
        Resume();
    }

    void OnPlay()
    {
        var (player, playable) = queuedPlayables.Dequeue();
        if (player.State == PlayerInfo.PlayerState.Dead && playable is not Trigger)
        {
            Debug.Log($"{player.Name} is dead. skipped {playable.GetType().Name}");
            return;
        }

        IEnumerator PlayAndResume()
        {
            currentPlayable = playable;
            yield return playable.Play(this, Map, currentUnit, player);
            currentPlayable = null;
            if (queuedPlayables.Count > 0)
                CurrentStage = Stage.Play;
            else if (currentUnit == null)
                CurrentStage = Stage.Events;
            else
                CurrentStage = Stage.Plan;

            Resume();
        }
        WaitingForAction = true;
        StartCoroutine(PlayAndResume());

        turnPanel.OnRoundStart();
    }

    void OnEvents()
    {
        if (currentUnit != null)
            turnPanel.OnTurnEnd(currentUnit);
        currentUnit = null;
        cardsPlayedForCurrentUnit = 0;
        var triggers = Map.CheckTriggers();
        if (triggers.Count > 0)
        {
            foreach (var trigger in triggers)
                QueuePlayable(trigger.player, trigger.trigger, false);
        }
        foreach (var player in CheckDeath(Map))
        {
            foreach (var unit in FindObjectsOfType<Unit>())
                if (unit.Player == player)
                    Destroy(unit);
            player.State = PlayerInfo.PlayerState.Dead;
            QueuePlayable(player, player.OnLooseTrigger, true);
        }

        if (queuedPlayables.Count() > 0)
            CurrentStage = Stage.Play;
        else if (playableUnits.Count() > 0)
        {
            CurrentStage = Stage.PreparePlan;
        }
        else if (RegisteredPlayers.Count() > 0)
        {
            CurrentStage = Stage.StartTurn;
        }
        else
        {
            CurrentStage = Stage.End;
        }
    }

    void OnEnd()
    {
        Debug.Log("No Player left! Game ended!");
        LoadingScreen.GoToMenu();
    }

    Unit currentUnit;
    Queue<Unit> playableUnits;
    void OnStartTurn()
    {
        foreach (var player in RegisteredPlayers)
        {
            player.Deck.OnTurnEnd(Map, player);
            player.Deck.OnTurnStart(Map, player);
        }

        playableUnits.Clear();
        var playables = new List<Unit>();
        foreach (var unit in FindObjectsOfType<Unit>())
            if (unit.Player != null && RegisteredPlayers.Contains(unit.Player))
            {
                playables.Add(unit);
            }
        playables.Sort((x, y) => x.Speed.CompareTo(y.Speed));
        foreach (var item in playables)
        {
            playableUnits.Enqueue(item);
        }
        turnPanel.SortBadges();

        if (playableUnits.Count() > 0)
            CurrentStage = Stage.PreparePlan;
        else
            CurrentStage = Stage.End;
    }

    void OnPreparePlan()
    {
        currentUnit = playableUnits.Dequeue();
        cardsPlayedForCurrentUnit = 0;
        WaitingForAction = true;
        StartCoroutine(turnPanel.OnTurnStart(currentUnit, onComplete: Resume));

        CurrentStage = Stage.Plan;
    }

    int cardsPlayedForCurrentUnit;
    void OnPlan()
    {
        var player = currentUnit.Player;
        WaitingForAction = true;
        if (player.IsAI)
            AIPrepare(player);
        else
            PlayerPrepare(player);
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
            turnPanel = GameObject.FindObjectOfType<TurnPanel>();
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
            Debug.Log($"Queued {card.Name} for {player.Name}");
        }
        else
            Debug.Log($"Queued {playable.GetType().Name} for {player.Name}");

        queuedPlayables.Enqueue((player, playable));
        if (priorizeNext)
            while (!queuedPlayables.Peek().Equals((player, playable)))
                queuedPlayables.Enqueue(queuedPlayables.Dequeue());
    }

    void Update()
    {
        if (!WaitingForAction)
        {
            Debug.Log("-- " + CurrentStage + " --");
            switch (CurrentStage)
            {
                case Stage.PreparePlan:
                    OnPreparePlan(); break;
                case Stage.Plan:
                    OnPlan(); break;
                case Stage.Play:
                    OnPlay(); break;
                case Stage.StartTurn:
                    OnStartTurn(); break;
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
        if (aiPlayer.Deck.Playable.Count() > 0)
        {
            var rnd = Random.Range(0, aiPlayer.Deck.Playable.Count());
            SetPlayerReady(aiPlayer, aiPlayer.Deck.Playable.ToArray()[rnd]);
        }
        else
            SetPlayerDone(aiPlayer);

    }

    void PlayerPrepare(PlayerInfo player)
    {
        if (player.Deck.Playable.Count() > 0)
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player, currentUnit,
            onPlay: (card) => SetPlayerReady(player, card),
            onEndTurn: () => SetPlayerDone(player));
        else
            SetPlayerDone(player);
    }

    public void SetPlayerDone(PlayerInfo player)
    {
        if (cardsPlayedForCurrentUnit == 0)
        {
            Debug.LogWarning("Error with Queue. Ouch!");
            QueuePlayable(player, player.OnInvalidQueueTrigger, true);
        }

        CurrentStage = Stage.Events;
        Resume();
    }

    public void SetPlayerReady(PlayerInfo player, Card card)
    {
        QueuePlayable(player, card, false);
        cardsPlayedForCurrentUnit++;
        CurrentStage = Stage.Play;
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
        return deadPlayers;
    }
}
