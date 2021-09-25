using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlate : MonoBehaviour
{
    [SerializeField] RectTransform readyPanel;
    [SerializeField] RectTransform nextActionsPanel;

    List<(PlayerInfo, Card)> cardsThisRound;
    Queue<(PlayerInfo, Card)> cardsToPlay;
    (PlayerInfo, Card) currentAction;

    [SerializeField] PlayerInfoPanel playerInfoPanelPrefab;

    [SerializeField] CardDisplay turnIndicatorPrefab;

    Dictionary<PlayerInfo, PlayerInfoPanel> registeredPlayers;

    public void Register(PlayerInfo player)
    {
        if (registeredPlayers == null)
            registeredPlayers = new Dictionary<PlayerInfo, PlayerInfoPanel>();

        if (registeredPlayers.ContainsKey(player)) return;

        var badge = GameObject.Instantiate(playerInfoPanelPrefab);
        badge.Player = player;
        badge.transform.SetParent(readyPanel);

        registeredPlayers.Add(player, badge);
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
    void PlayerPrepare(PlayerInfo player, PlayerInfoPanel panel)
    {
        if (player.Deck.Commander?.State == Card.CardState.Unplayed)
            SetPlayerReady(player, new Card[1] { player.Deck.Commander });
        else
            GameObject.FindObjectOfType<PlanningPanel>(true).Show(player, panel);
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
                    PlayerPrepare(kvp.Key, kvp.Value);
                return true;
            }
        }
        return false;
    }

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerReady(PlayerInfo player, Card[] cards)
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
        var board = GameObject.FindObjectOfType<Board>();
        if (cardsThisRound.Count > 0)
        {
            // Round started
            cardsThisRound.Sort((x, y) => x.Item2.Speed.CompareTo(y.Item2.Speed));
            readyPanel.gameObject.SetActive(false);
            gameObject.SetActive(true);
            foreach (Transform t in nextActionsPanel)
                Destroy(t.gameObject);

            foreach (var item in cardsThisRound)
            {
                cardsToPlay.Enqueue(item);
                var indicator = Instantiate(turnIndicatorPrefab);
                indicator.GetComponent<SkillProgressBar>().SetSkills(item.Item2, item.Item1);
                indicator.SetCard(item.Item2);
                indicator.PlayerColorBorder.color = item.Item1.Color;
                indicator.transform.SetParent(nextActionsPanel);
            }
            cardsThisRound.Clear();
        }
        else
        {
            // Mid round
            var triggers = board.Map.CheckTriggers(CheckDeath(board.Map));
            if (triggers.Count > 0)
            {
                StartCoroutine(RunTriggers(board, triggers));
                return;
            }
        }

        if (cardsToPlay.Count > 0)
        {
            currentAction = cardsToPlay.Dequeue();
            board.SetTurn(currentAction.Item1, currentAction.Item2);
        }
        else
        {
            StartTurn();
        }
    }

    IEnumerator RunTriggers(Board board, List<(Tile tile, Trigger trigger)> triggers)
    {
        foreach (var trigger in triggers)
            yield return trigger.trigger.Execute(board.Map, trigger.tile);
        Progress();
    }

    List<PlayerInfo> CheckDeath(Map map)
    {
        var deadPlayers = new List<PlayerInfo>();
        foreach (var kvp in registeredPlayers)
        {
            var player = kvp.Key;
            if (player.LooseCondition.Check(map, player))
            {
                deadPlayers.Add(player);
            }
        }
        foreach (var player in deadPlayers)
        {
            Debug.Log("A player died!");
            Destroy(registeredPlayers[player].gameObject);
            registeredPlayers.Remove(player);
        }
        return deadPlayers;
    }

    internal void StartTurn()
    {
        cardsThisRound = new List<(PlayerInfo, Card)>();
        cardsToPlay = new Queue<(PlayerInfo, Card)>();
        currentAction = (default, null);
        gameObject.SetActive(false);
        readyPanel.gameObject.SetActive(true);

        var map = GameObject.FindObjectOfType<Board>().Map;
        foreach (var kvp in registeredPlayers)
        {
            var player = kvp.Key;

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
                    pcard.Passive.OnRoundStart(map, player);
                }
            }

            kvp.Value.UpdateValues();
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
