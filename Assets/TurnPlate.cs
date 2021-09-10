using System;
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
    [SerializeField] RectTransform actionsPanel;
    [SerializeField] RectTransform currentActionPanel;

    int playersTotal, playersReadyMask;
    private TurnState state;

    public bool TurnComplete(int player) => 0 != (playersReadyMask & (1 << player));

    public static TurnPlate Current { get; private set; }

    List<(int, Card)> cardsThisRound;
    Queue<(int, Card)> cardsToPlay;
    (int, Card) currentAction;

    [SerializeField] RectTransform playerUnreadyBadgePrefab;

    public void RegisterPlayer(int player)
    {
        var badge = GameObject.Instantiate(playerUnreadyBadgePrefab);
        badge.GetComponentInChildren<Image>().color = Board.PlayerColor(player);
        badge.SetParent(readyPanel);
        badge.SetSiblingIndex(player);
    }

    public void SetPlayerReady(int player, Card[] cards)
    {
        foreach (Transform badge in readyPanel)
        {
            if (badge.GetComponentInChildren<Image>().color == Board.PlayerColor(player))
                badge.gameObject.SetActive(false);
        }

        foreach (var card in cards)
        {
            cardsThisRound.Add((player, card));
        }
        foreach (Transform badge in readyPanel)
        {
            if (badge.gameObject.activeSelf)
                return;
        }
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
                var indicator = item.Item2.TurnIndicator(Board.PlayerColor(item.Item1));
                indicator.SetParent(actionsPanel);
                indicator.sizeDelta = new Vector2(80, 75);
            }
            actionsPanel.parent.gameObject.SetActive(true);
            cardsThisRound.Clear();
        }

        if (cardsToPlay.Count > 0)
        {
            currentAction = cardsToPlay.Dequeue();
            if (currentActionPanel.childCount > 0)
                Destroy(currentActionPanel.GetChild(0).gameObject);
            actionsPanel.GetChild(0).SetParent(currentActionPanel);
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
        Current = this;
        state = TurnState.Planning;
        cardsThisRound = new List<(int, Card)>();
        cardsToPlay = new Queue<(int, Card)>();
        currentAction = (0, null);
        actionsPanel.parent.gameObject.SetActive(false);
        readyPanel.gameObject.SetActive(true);
        foreach (Transform child in readyPanel)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void Next()
    {
        Progress();
    }
}
