using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    TurnPlate turnPanel;
    Map Map;


    List<(PlayerInfo, Card)> cardsThisRound;
    Queue<(PlayerInfo, Card)> cardsToPlay;
    (PlayerInfo, Card) currentAction;

    List<PlayerInfo> registeredPlayers;
    List<PlayerInfo> readyPlayers;

    // Start is called before the first frame update
    public void Initialize(Map mapPrefab)
    {
        Map = Map.Create(mapPrefab);
        Map.RevealArea(0);
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

        turnPanel.AddPlayer(player);
        registeredPlayers.Add(player);
    }

    void AIPrepare(PlayerInfo aiPlayer)
    {
        foreach (var card in aiPlayer.Deck.Cards)
        {
            if (card.State == Card.CardState.Unplayed)
            {
                SetPlayerReady(aiPlayer, new Card[1] { card });
                return;
            }
        }
    }
    void PlayerPrepare(PlayerInfo player)
    {
        if (player.Deck.Commander?.State == Card.CardState.Unplayed)
            SetPlayerReady(player, new Card[1] { player.Deck.Commander });
        else
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player, turnPanel.PlateOf(player));
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

    public void SetPlayerReady(PlayerInfo player, Card[] cards)
    {
        readyPlayers.Add(player);

        foreach (var card in cards)
        {
            cardsThisRound.Add((player, card));
        }

        if (NextPlayerPrepareTurn())
            return;

        // Round started
        cardsThisRound.Sort((x, y) => x.Item2.Speed.CompareTo(y.Item2.Speed));
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
        else
        {
            PrepareNextRound();
        }
    }

    IEnumerator RunTriggers(List<(Tile tile, Trigger trigger)> triggers)
    {
        foreach (var trigger in triggers)
            yield return trigger.trigger.Execute(Map, trigger.tile);
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
            Debug.Log("A player died!");
            registeredPlayers.Remove(player);
            turnPanel.RemovePlayer(player);
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
            {
                if (card is ActionCard acard && acard.Temporary && acard.State == Card.CardState.Unplayed) acard.Destroy();
            }
            player.Deck.ShuffleIfNeccessary();
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
