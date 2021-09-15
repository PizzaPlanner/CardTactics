using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlate : MonoBehaviour
{
    [SerializeField] RectTransform readyPanel;
    [SerializeField] RectTransform nextActionsPanel;
    [SerializeField] RectTransform actionsPanel;

    List<(GameSettings.PlayerInfo, Card)> cardsThisRound;
    Queue<(GameSettings.PlayerInfo, Card)> cardsToPlay;
    (GameSettings.PlayerInfo, Card) currentAction;

    [SerializeField] PlayerInfoPanel playerInfoPanelPrefab;

    [SerializeField] CardDisplay turnIndicatorPrefab;

    Dictionary<GameSettings.PlayerInfo, PlayerInfoPanel> registeredPlayers;

    public void Register(GameSettings.PlayerInfo player)
    {
        if (registeredPlayers == null)
            registeredPlayers = new Dictionary<GameSettings.PlayerInfo, PlayerInfoPanel>();

        if (registeredPlayers.ContainsKey(player)) return;

        var badge = GameObject.Instantiate(playerInfoPanelPrefab);
        badge.Player = player;
        badge.transform.SetParent(readyPanel);

        registeredPlayers.Add(player, badge);
    }

    void AIPrepare(GameSettings.PlayerInfo aiPlayer)
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
    void PlayerPrepare(GameSettings.PlayerInfo player)
    {
        if (player.Deck.Commander?.State == Card.CardState.Unplayed)
            SetPlayerReady(player, new Card[1] { player.Deck.Commander });
        else
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player);
    }

    public bool NextPlayerPrepareTurn()
    {
        foreach (var kvp in registeredPlayers)
        {
            if (!kvp.Value.IsReady)
            {
                if (kvp.Key.IsAI)
                    AIPrepare(kvp.Key);
                else
                    PlayerPrepare(kvp.Key);
                return true;
            }
        }
        return false;
    }

    void Awake()
    {
        actionsPanel.gameObject.SetActive(false);
    }

    public void SetPlayerReady(GameSettings.PlayerInfo player, Card[] cards)
    {
        registeredPlayers[player].IsReady = true;

        foreach (var card in cards)
        {
            cardsThisRound.Add((player, card));
        }

        if (NextPlayerPrepareTurn())
            return;
        Progress();
    }

    void Progress()
    {
        if (cardsThisRound.Count > 0)
        {
            cardsThisRound.Sort((x, y) => x.Item2.Speed.CompareTo(y.Item2.Speed));
            readyPanel.gameObject.SetActive(false);
            foreach (var item in cardsThisRound)
            {
                cardsToPlay.Enqueue(item);
                var indicator = Instantiate(turnIndicatorPrefab);
                indicator.SetCard(item.Item2);
                indicator.PlayerColorBorder.color = item.Item1.Color;
                indicator.transform.SetParent(nextActionsPanel);
            }
            cardsThisRound.Clear();
        }

        actionsPanel.gameObject.SetActive(false);
        var board = GameObject.FindObjectOfType<Board>();
        board.Paused = true;
        StartCoroutine(board.Settings.Map.CheckTriggers(board.Settings, () =>
        {
            board.Paused = false;
            if (cardsToPlay.Count > 0)
            {
                currentAction = cardsToPlay.Dequeue();
                Destroy(nextActionsPanel.GetChild(0).gameObject);
                board.SetTurn(currentAction.Item1, currentAction.Item2);
                actionsPanel.gameObject.SetActive(true);
            }
            else
            {
                StartTurn();
            }
        }));
    }

    internal void StartTurn()
    {
        cardsThisRound = new List<(GameSettings.PlayerInfo, Card)>();
        cardsToPlay = new Queue<(GameSettings.PlayerInfo, Card)>();
        currentAction = (default, null);
        actionsPanel.gameObject.SetActive(false);
        readyPanel.gameObject.SetActive(true);

        List<GameSettings.PlayerInfo> deadPlayers = new List<GameSettings.PlayerInfo>();

        var settings = GameObject.FindObjectOfType<Board>().Settings;
        foreach (var kvp in registeredPlayers)
        {
            var player = kvp.Key;
            if (player.LooseCondition.Check(settings, player))
            {
                deadPlayers.Add(player);
                continue;
            }

            kvp.Value.gameObject.SetActive(true);
            kvp.Value.IsReady = false;

            foreach (Card card in player.Deck.Cards)
            {
                if (card is ActionCard acard && acard.Temporary && acard.State == Card.CardState.Unplayed) acard.Destroy();
            }
            player.Deck.ShuffleIfNeccessary();
            foreach (Card card in player.Deck.Cards)
            {
                if (card is not PassiveCard pcard) continue;
                if (pcard.State == Card.CardState.Active)
                {
                    pcard.Passive.OnRoundStart(settings, player);
                }
            }

            kvp.Value.UpdateValues();
        }

        foreach (var player in deadPlayers)
        {
            Debug.Log("A player died!");
            Destroy(registeredPlayers[player].gameObject);
            registeredPlayers.Remove(player);
        }

        if (!NextPlayerPrepareTurn())
        {
            Debug.Log("No Player left! Game ended!");
            LoadingScreen.GoToMenu();
        }
    }

    public void Next()
    {
        Progress();
    }
}
