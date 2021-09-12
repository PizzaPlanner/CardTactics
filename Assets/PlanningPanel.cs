using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningPanel : MonoBehaviour
{
    [SerializeField] Text Name, CardsLeft, CardsPlayed, CardsRemoved;

    [SerializeField] Text costText;
    [SerializeField] RectTransform cardsPanel;
    [SerializeField] Button confirm;

    int RoundPointsSpent;

    public void Show(GameSettings.PlayerInfo player)
    {
        var deck = player.Deck;
        gameObject.SetActive(true);

        Name.text = deck.name;
        Name.color = player.Color;

        var selection = new List<Card>();
        var cardsToPlay = new List<Card>();
        var cardsPlayed = new List<Card>();
        var cardsDestroyed = new List<Card>();
        foreach (var card in deck.Cards)
        {
            switch (card.State)
            {
                case Card.CardState.Unplayed:
                    cardsToPlay.Add(card); break;
                case Card.CardState.Active:
                case Card.CardState.Played:
                    cardsPlayed.Add(card); break;
                case Card.CardState.Destroyed:
                    cardsDestroyed.Add(card); break;
            }
        }
        RoundPointsSpent = 0;
        costText.text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
        confirm.interactable = false;
        if (cardsToPlay.Count == 0)
        {
            cardsToPlay.AddRange(cardsPlayed);
            foreach (var card in cardsPlayed)
                card.State = Card.CardState.Unplayed;
            cardsPlayed.Clear();
        }
        cardsToPlay.Sort((x, y) => x.Speed.CompareTo(y.Speed));

        CardsLeft.text = cardsToPlay.Count.ToString();
        CardsPlayed.text = cardsPlayed.Count.ToString();
        CardsRemoved.text = cardsDestroyed.Count.ToString();

        confirm.onClick.RemoveAllListeners();
        confirm.onClick.AddListener(() =>
        {
            Hide();
            GameObject.FindObjectOfType<TurnPlate>().SetPlayerReady(player, selection.ToArray());
            foreach (var card in selection)
            {
                if (card.Once)
                    card.State = Card.CardState.Destroyed;
                else
                    card.State = Card.CardState.Played;
            }
        });

        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var cardPrefab in cardsToPlay)
        {
            var card = cardPrefab.FullCard();
            card.SetParent(cardsPanel);
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selection.Contains(cardPrefab))
                {
                    selection.Remove(cardPrefab);
                    RoundPointsSpent -= cardPrefab.Cost;
                }
                else
                {
                    RoundPointsSpent += cardPrefab.Cost;
                    selection.Add(cardPrefab);
                }
                card.GetComponent<Button>().animator.SetBool("IsActive", selection.Contains(cardPrefab));
                confirm.interactable = selection.Count != 0 && RoundPointsSpent <= deck.RoundPointsFree;
                costText.text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
                foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardReference>(false))
                {
                    if (selection.Contains(otherCard.Reference) || RoundPointsSpent + otherCard.Reference.Cost <= deck.RoundPointsFree)
                    {
                        otherCard.GetComponent<Button>().interactable = true;
                        otherCard.Cost.color = Color.white;
                    }
                    else
                    {
                        otherCard.GetComponent<Button>().interactable = false;
                        otherCard.Cost.color = Color.red;
                    }
                }
            });
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
