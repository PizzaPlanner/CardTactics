using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

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

    readonly Queue<IPlayableContext> queue;

    public IPlayableContext Dequeue() => queue.Dequeue();
    public void Enqueue(IPlayableContext context) => Enqueue(context, false);
    public void InsertFirst(IPlayableContext context) => Enqueue(context, true);


    void Enqueue(IPlayableContext context, bool priorizeNext)
    {
        if (!context.Queueable)
        {
            Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} for {context.Player.Name}");
            return;
        }
        Debug.Log($"[Playable] Queue {context.Name} for {context.Player.Name}");

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

public class Board : MonoBehaviour
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

    [SerializeField] GameSettings settings;

    [ShowInInspector]
    bool WaitingForAction;

    [ShowInInspector]
    public Stage CurrentStage { get; private set; }

    TurnPanel turnPanel;
    Map Map;

    [ShowInInspector]
    public PlayableQueue Playables { get; } = new();

    IEnumerable<PlayerInfo> RegisteredPlayers => Map.Players.Where(x => x.IsAlive);

    [SerializeField]
    Card OnNoCardsPlayed;

    Lobby lobby;

    // Start is called before the first frame update
    public void Initialize(Map mapInstance, Lobby lobby)
    {
        CurrentStage = Stage.Setup;
        playableUnits = new Queue<Unit>();
        this.lobby = lobby;
        settings.Load();
        Map = mapInstance;
        Map.Load(this);
        WaitingForAction = true;
    }

    public void StartGame()
    {
        CurrentStage = Stage.Events;
        Resume();
    }

    void OnPlay()
    {
        var ctx = Playables.Dequeue();

        IEnumerator PlayAndResume()
        {
            currentContext = ctx;
            Debug.Log($"[Playable] Play {ctx.Name} for {ctx.Player.Name}");
            yield return new WaitForEndOfFrame();
            yield return ctx.Play();
            currentContext = null;
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
        Map.AddTriggers(this);
        CheckDeath(Map);

        if (!Playables.Empty)
            CurrentStage = Stage.Play;
        else if (CurrentUnit != null)
            CurrentStage = Stage.Plan;
        else if (playableUnits.Count() > 0)
            CurrentStage = Stage.StartTurn;
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
        var survivors = Map.Players.Where(x => !x.Team.LooseCondition.Check(Map, x.Team)).Select(x => x.Team).Distinct();
        foreach (var player in Map.Players.Where(x => !x.IsAI && survivors.Contains(x.Team) && survivors.Count() == 1))
            lobby.Award($"Victory:{Map.name}", player);
        lobby.Award($"Finished:{Map.name}");
        lobby.Return();
    }


    public Unit CurrentUnit { get; private set; }
    Queue<Unit> playableUnits;
    int round;
    void OnStartRound()
    {
        foreach (var player in RegisteredPlayers)
        {
            player.Deck.OnRoundEnd();
            player.Deck.OnRoundStart();
        }

        playableUnits.Clear();
        var playables = new List<Unit>();
        foreach (var unit in FindObjectsOfType<Unit>())
            if (unit.Player != null && RegisteredPlayers.Contains(unit.Player))
            {
                unit.Resources.Refresh();
                playables.Add(unit);
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
        StartCoroutine(turnPanel.OnRoundStart(++round, onComplete: Resume));
    }

    void OnEndTurn()
    {
        turnPanel.OnTurnEnd(CurrentUnit);
        Debug.Log($"[Board] End Turn of {CurrentUnit.Name}");
        if (CurrentUnit.Player != null)
            CurrentUnit.Player.Deck.OnTurnEnd();
        CurrentUnit = null;
        if (Playables.Empty)
            CurrentStage = Stage.Events;
        else
            CurrentStage = Stage.Play;
    }

    void OnStartTurn()
    {
        CurrentUnit = playableUnits.Dequeue();
        Debug.Log($"[Board] Start Turn of {CurrentUnit.Name}");
        cardsPlayedForCurrentUnit = 0;
        WaitingForAction = true;
        StartCoroutine(turnPanel.OnTurnStart(CurrentUnit, onComplete: Resume));
        if (CurrentUnit.Player != null)
            CurrentUnit.Player.Deck.OnTurnStart();
        if (Playables.Empty)
            CurrentStage = Stage.Plan;
        else
            CurrentStage = Stage.Play;
    }

    int cardsPlayedForCurrentUnit;
    void OnPlan()
    {
        var player = CurrentUnit.Player;
        WaitingForAction = true;
        if (player.IsAI)
            AIPrepare(player);
        else
            PlayerPrepare(player);
    }

    IPlayableContext currentContext;
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
        else if (CurrentStage == Stage.Play && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && Tile.Hovered != null)
            {
                if (Tile.Hovered.CurrentIndicator == TargetIndicator.Valid)
                    Map.OnTileSelection?.Invoke(Tile.Hovered.PrimaryPattern);
            }
        }

        if (WaitingForAction)
            switch (CurrentStage)
            {
                case Stage.Plan:
                case Stage.Play:
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
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player, CurrentUnit,
            onPlay: (card) => SetPlayerReady(player, card),
            onEndTurn: () => SetPlayerDone(player));
        else
            SetPlayerDone(player);
    }

    public void SetPlayerDone(PlayerInfo player)
    {
        if (cardsPlayedForCurrentUnit == 0)
        {
            Debug.LogWarning($"{CurrentUnit.Name} ended without playing a Card!");
            var oncp = new CardContext(OnNoCardsPlayed, Map, CurrentUnit, CardState.Unplayed);
            Playables.InsertFirst(oncp);
        }

        CurrentStage = Stage.EndTurn;
        Resume();
    }

    public void SetPlayerReady(PlayerInfo player, CardContext card)
    {
        Playables.Enqueue(card);
        cardsPlayedForCurrentUnit++;
        CurrentStage = Stage.Play;
        Resume();
    }

    void CheckDeath(Map map)
    {
        foreach (var player in map.Players)
        {
            if (player.Team.LooseCondition.Check(map, player.Team) && player.Team.OnLooseTrigger.ShouldTrigger(map, map.TileOf(player.Deck.Commander)))
                Playables.InsertFirst(new TriggerContext(player.Deck.Commander, map, this, player.Team.OnLooseTrigger));
        }
    }

    internal CardContext CreateFakeCardContext()
    {
        return new CardContext(new ActionCard(), Map, null, CardState.Unplayed);
    }
}
