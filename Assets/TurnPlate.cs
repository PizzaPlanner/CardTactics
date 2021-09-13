using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlate : MonoBehaviour
{
    enum TurnState
    {
        Planning,
        Started,
        Acting
    }
    [SerializeField] RectTransform readyPanel;
    [SerializeField] RectTransform nextActionsPanel;
    [SerializeField] RectTransform actionsPanel;
    [SerializeField] RectTransform currentActionPanel;

    private TurnState state;

    List<(GameSettings.PlayerInfo, Card)> cardsThisRound;
    Queue<(GameSettings.PlayerInfo, Card)> cardsToPlay;
    (GameSettings.PlayerInfo, Card) currentAction;

    [SerializeField] PlayerPendingBadge playerUnreadyBadgePrefab;

    [SerializeField] CardDisplay turnIndicatorPrefab;

    public void Register(GameSettings.PlayerInfo player)
    {
        var badge = GameObject.Instantiate(playerUnreadyBadgePrefab);
        badge.Player = player;
        badge.transform.SetParent(readyPanel);
    }

    public bool NextPlayerPrepareTurn()
    {
        var next = GetComponentInChildren<PlayerPendingBadge>(false);
        if (!next) return false;
        GameObject.FindObjectOfType<PlanningPanel>(true).Show(next.Player);
        return true;
    }

    public void SetPlayerReady(GameSettings.PlayerInfo player, Card[] cards)
    {
        foreach (Transform badge in readyPanel)
        {
            if (badge.GetComponentInChildren<Image>().color == player.Color)
                badge.gameObject.SetActive(false);
        }

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
                indicator.transform.SetParent(nextActionsPanel);
            }
            actionsPanel.gameObject.SetActive(true);
            cardsThisRound.Clear();
        }

        if (cardsToPlay.Count > 0)
        {
            currentAction = cardsToPlay.Dequeue();
            if (currentActionPanel.childCount > 0)
                Destroy(currentActionPanel.GetChild(0).gameObject);
            nextActionsPanel.GetChild(0).SetParent(currentActionPanel);
            GameObject.FindObjectOfType<Board>().SetTurn(currentAction.Item1, currentAction.Item2);
            state = TurnState.Acting;
        }
        else
        {
            StartTurn();
        }
    }

    internal void StartTurn()
    {
        state = TurnState.Planning;
        cardsThisRound = new List<(GameSettings.PlayerInfo, Card)>();
        cardsToPlay = new Queue<(GameSettings.PlayerInfo, Card)>();
        currentAction = (default, null);
        actionsPanel.gameObject.SetActive(false);
        readyPanel.gameObject.SetActive(true);
        foreach (Transform child in readyPanel)
        {
            child.gameObject.SetActive(true);
        }

        var settings = GameObject.FindObjectOfType<Board>().Settings;
        foreach (var player in settings.Players)
        {
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
        }


        NextPlayerPrepareTurn();
    }

    public void Next()
    {
        Progress();
    }
}
