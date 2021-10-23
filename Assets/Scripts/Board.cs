using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    TurnPlate turnPanel;
    Map Map;


    List<(PlayerInfo, Card)> cardsThisRound;
    Queue<(PlayerInfo, Card)> cardsToPlay;
    (PlayerInfo, Card) currentAction;

    [ShowInInspector]
    [ReadOnly]
    List<PlayerInfo> registeredPlayers;

    List<PlayerInfo> readyPlayers;

    bool beforeSecondTeamAdded;

    // Start is called before the first frame update
    public void Initialize(Map mapPrefab)
    {
        Map = Instantiate(mapPrefab);
        beforeSecondTeamAdded = true;
        foreach (var player in Map.Players)
        {
            if (!player.RegisterLater)
            {
                Register(player);
            }
        }
    }

    public void StartGame()
    {
        PrepareNextRound();
    }

    void Awake()
    {
        if (Map == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
        else
            turnPanel = GameObject.FindObjectOfType<TurnPlate>();
    }

    Card currentCard;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && currentCard != null)
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

    void NextTurn()
    {
        currentCard = null;
        Map.HideTileSelectionPreview();
        Progress();
    }

    internal void SetTurn(PlayerInfo player, Card card)
    {
        currentCard = card;
        var cr = StartCoroutine(currentCard.Play(Map, player, onComplete: NextTurn));
    }


    public void Register(PlayerInfo player)
    {
        if (registeredPlayers == null)
            registeredPlayers = new List<PlayerInfo>();

        if (registeredPlayers.Contains(player)) return;

        player.Initialize();
        turnPanel.AddPlayer(player);
        registeredPlayers.Add(player);

        if (registeredPlayers.GroupBy(x => x.Team).Count() > 1)
            beforeSecondTeamAdded = false;
    }

    void AIPrepare(PlayerInfo aiPlayer)
    {
        foreach (var card in aiPlayer.Deck.Cards)
        {
            if (card.State == Card.CardState.Unplayed)
            {
                card.SetSelectionState(true);
                SetPlayerReady(aiPlayer);
                return;
            }
        }
    }

    void PlayerPrepare(PlayerInfo player)
    {
        if (player.Deck.Commander?.State == Card.CardState.Queued)
            SetPlayerReady(player);
        else
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player);
    }

    public bool NextPlayerPrepareTurn()
    {
        foreach (var kvp in registeredPlayers)
        {
            if (readyPlayers.Contains(kvp)) continue;
            if (kvp.IsAI)
                AIPrepare(kvp);
            else
                PlayerPrepare(kvp);
            return true;
        }
        return false;
    }

    public void SetPlayerReady(PlayerInfo player)
    {
        readyPlayers.Add(player);

        foreach (var card in player.Deck.Queued)
        {
            cardsThisRound.Add((player, card));
        }

        turnPanel.OnPlayerReady(player);
        if (NextPlayerPrepareTurn())
            return;

        // Round started
        cardsThisRound.Sort((x, y) =>
        {
            var deckComp = x.Item1.Deck.CurrentSpeed.CompareTo(y.Item1.Deck.CurrentSpeed);
            if (deckComp == 0)
                return x.Item2.Speed.CompareTo(y.Item2.Speed);
            else
                return deckComp;
        });
        turnPanel.OnRoundStart();
        foreach (var item in cardsThisRound)
        {
            cardsToPlay.Enqueue(item);
            turnPanel.OnCardInQueue(item.Item2, item.Item1);
        }
        cardsThisRound.Clear();
        Progress();
    }

    void Progress()
    {
        var triggers = Map.CheckTriggers(CheckDeath(Map));
        if (triggers.Count > 0)
        {
            StartCoroutine(RunTriggers(triggers));
            return;
        }

        if (cardsToPlay.Count > 0)
        {
            currentAction = cardsToPlay.Dequeue();
            SetTurn(currentAction.Item1, currentAction.Item2);
        }
        else if (registeredPlayers.GroupBy(x => x.Team).Count() > 1 || beforeSecondTeamAdded)
        {
            PrepareNextRound();
        }
        else
        {
            foreach (var player in registeredPlayers)
                Debug.Log($"<color={player.Color}>{player.Deck.Commander.Name}</color> won!");
            LoadingScreen.GoToMenu();
        }
    }

    IEnumerator RunTriggers(List<(Tile tile, Trigger trigger)> triggers)
    {
        foreach (var trigger in triggers)
            yield return trigger.trigger.Run(Map, trigger.tile);
        Progress();
    }

    List<PlayerInfo> CheckDeath(Map map)
    {
        var deadPlayers = new List<PlayerInfo>();
        foreach (var player in registeredPlayers)
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
            registeredPlayers.Remove(player);
        }
        return deadPlayers;
    }

    internal void PrepareNextRound()
    {
        cardsThisRound = new List<(PlayerInfo, Card)>();
        cardsToPlay = new Queue<(PlayerInfo, Card)>();
        currentAction = (default, null);
        readyPlayers = new List<PlayerInfo>();

        var map = GameObject.FindObjectOfType<Board>().Map;
        foreach (var player in registeredPlayers)
        {
            foreach (Card card in player.Deck.Cards)
                card.OnRoundStart(map, player);
        }

        turnPanel.OnPreparation();

        if (!NextPlayerPrepareTurn())
        {
            Debug.Log("No Player left! Game ended!");
            LoadingScreen.GoToMenu();
        }
    }
}
