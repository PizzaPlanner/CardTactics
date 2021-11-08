using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayableQueue
{
    public struct PlayableWithInfo
    {
        public PlayerInfo Player;
        public Unit Unit;
        public IPlayable Playable;
    }

    public PlayableQueue()
    {
        queue = new();
    }

    public bool Empty => queue.Count == 0;

    readonly Queue<PlayableWithInfo> queue;

    public PlayableWithInfo Dequeue() => queue.Dequeue();
    public void Enqueue(PlayerInfo player, Unit unit, IPlayable playable) => Enqueue(player, unit, playable, false);
    public void InsertFirst(PlayerInfo player, Unit unit, IPlayable playable) => Enqueue(player, unit, playable, true);


    void Enqueue(PlayerInfo player, Unit unit, IPlayable playable, bool priorizeNext)
    {
        if (playable is Card card)
        {
            if (card.State != Card.CardState.Unplayed)
            {
                Debug.LogError($"Failed to Queue {card.Name}: State is {card.State}");
                return;
            }
            Debug.Log($"Queued {card.Name} for {player.Name}");
        }
        else
            Debug.Log($"Queued {playable.GetType().Name} for {player.Name}");

        if (priorizeNext)
        {
            var old = queue.ToArray();
            queue.Clear();
            queue.Enqueue(new PlayableWithInfo
            {
                Player = player,
                Unit = unit,
                Playable = playable
            });
            foreach (var item in old)
                queue.Enqueue(item);
        }
        else
            queue.Enqueue(new PlayableWithInfo { Player = player, Unit = unit, Playable = playable });
    }
}

public class Board : MonoBehaviour
{

    public enum Stage
    {
        Setup,
        PreparePlan,
        Plan,
        Play,
        EndTurn,
        StartRound,
        Events,
        End

    }

    public float CardPlayPreTime;
    public float CardPlayPostTime;

    [ShowInInspector]
    bool WaitingForAction;

    [ShowInInspector]
    public Stage CurrentStage { get; private set; }

    TurnPanel turnPanel;
    Map Map;

    [ShowInInspector]
    public PlayableQueue Playables { get; } = new();

    IEnumerable<PlayerInfo> RegisteredPlayers => Map.Players.Where(x => x.State == PlayerInfo.PlayerState.Active);

    [SerializeField]
    Card OnNoCardsPlayed;

    Lobby lobby;

    // Start is called before the first frame update
    public void Initialize(Map mapInstance, Lobby lobby)
    {
        CurrentStage = Stage.Setup;
        playableUnits = new Queue<Unit>();
        this.lobby = lobby;
        Map = mapInstance;
        Map.Load(this);
        WaitingForAction = true;
    }

    public bool Register(PlayerInfo player)
    {
        if (player.State != PlayerInfo.PlayerState.Hidden) return false;
        player.State = PlayerInfo.PlayerState.Dead;
        if (!player.Initialize(lobby))
            return false;

        player.State = PlayerInfo.PlayerState.Active;
        Playables.InsertFirst(player, null, player.Deck.Commander);
        return true;
    }

    public void StartGame()
    {
        CurrentStage = Stage.Events;
        Resume();
    }

    void OnPlay()
    {
        var info = Playables.Dequeue();
        var player = info.Player;
        var playable = info.Playable;
        var unit = info.Unit;
        if (player != null && player.State == PlayerInfo.PlayerState.Dead && playable is not Trigger)
        {
            Debug.Log($"{player.Name} is dead. skipped {playable.GetType().Name}");
            return;
        }

        IEnumerator PlayAndResume()
        {
            currentPlayable = playable;
            yield return playable.Play(this, Map, unit, player);
            currentPlayable = null;
            if (CurrentStage != Stage.End)
            {
                if (!Playables.Empty)
                    CurrentStage = Stage.Play;
                else
                    CurrentStage = Stage.Events;
            }

            Resume();
            turnPanel.OnPlayablePlayed();
        }
        WaitingForAction = true;
        StartCoroutine(PlayAndResume());
    }

    void OnEvents()
    {
        var triggers = Map.CheckTriggers();
        if (triggers.Count > 0)
        {
            foreach (var trigger in triggers)
                Playables.Enqueue(trigger.player, currentUnit, trigger.trigger);
        }
        foreach (var player in CheckDeath(Map))
        {
            foreach (var unit in FindObjectsOfType<Unit>())
                if (unit.Player == player)
                    Destroy(unit);
            player.State = PlayerInfo.PlayerState.Dead;
            Playables.InsertFirst(player, currentUnit, player.OnLooseTrigger);
        }

        if (!Playables.Empty)
            CurrentStage = Stage.Play;
        else if (currentUnit != null)
            CurrentStage = Stage.Plan;
        else if (playableUnits.Count() > 0)
            CurrentStage = Stage.PreparePlan;
        else if (RegisteredPlayers.Count() > 0)
            CurrentStage = Stage.StartRound;
        else
            CurrentStage = Stage.End;
    }

    public void CloseBoard()
    {
        CurrentStage = Stage.End;
    }

    void OnEnd()
    {
        WaitingForAction = true;
        var survivors = Map.Players.Where(x => !x.LooseCondition.Check(Map, x)).Select(x => x.Team).Distinct();
        foreach (var player in Map.Players.Where(x => !x.IsAI && survivors.Contains(x.Team) && survivors.Count() == 1))
            lobby.Award($"Victory:{Map.name}", player);
        lobby.Award($"Finished:{Map.name}");
        lobby.Return();
    }

    Unit currentUnit;
    Queue<Unit> playableUnits;
    int round;
    void OnStartRound()
    {
        foreach (var player in RegisteredPlayers)
        {
            player.Deck.OnRoundEnd(Map, player);
            player.Deck.OnRoundStart(Map, player);
        }

        playableUnits.Clear();
        var playables = new List<Unit>();
        foreach (var unit in FindObjectsOfType<Unit>())
            if (unit.Player != null && RegisteredPlayers.Contains(unit.Player))
            {
                unit.Resources.EnergySpent = 0;
                playables.Add(unit);
            }
        playables.Sort((x, y) => Unit.CompareSpeed(x, y));
        foreach (var item in playables)
        {
            playableUnits.Enqueue(item);
        }

        if (playableUnits.Count() > 0)
            CurrentStage = Stage.PreparePlan;
        else
            CurrentStage = Stage.End;

        WaitingForAction = true;
        StartCoroutine(turnPanel.OnRoundStart(++round, onComplete: Resume));
    }

    void OnEndTurn()
    {
        turnPanel.OnTurnEnd(currentUnit);
        currentUnit = null;
        CurrentStage = Stage.Events;
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
        if (aiPlayer.Deck.Playable(currentUnit).Count() > 0)
        {
            var rnd = Random.Range(0, aiPlayer.Deck.Playable(currentUnit).Count());
            SetPlayerReady(aiPlayer, aiPlayer.Deck.Playable(currentUnit).ToArray()[rnd]);
        }
        else
            SetPlayerDone(aiPlayer);
    }

    void PlayerPrepare(PlayerInfo player)
    {
        if (player.Deck.Playable(currentUnit).Count() > 0)
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
            Debug.LogWarning($"{currentUnit.Name} ended without playing a Card!");
            var onNoCardsInstance = Instantiate(OnNoCardsPlayed);
            onNoCardsInstance.Init();
            Playables.InsertFirst(player, currentUnit, onNoCardsInstance);
        }

        CurrentStage = Stage.EndTurn;
        Resume();
    }

    public void SetPlayerReady(PlayerInfo player, Card card)
    {
        Playables.Enqueue(player, currentUnit, card);
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
