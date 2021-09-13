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

    [SerializeField] RectTransform passivePanel;
    [SerializeField] PassiveIndicator passiveIndicatorPrefab;

    int RoundPointsSpent;

    public void Show(GameSettings.PlayerInfo player)
    {
        var deck = player.Deck;
        gameObject.SetActive(true);

        Name.text = deck.name;
        Name.color = player.Color;

        foreach (Transform t in passivePanel)
            GameObject.Destroy(t.gameObject);
        bool hasItems = false;
        foreach (var card in player.Deck.Cards)
        {
            if (card.State != Card.CardState.Active) continue;
            if (card is UnitCard commander && commander.Passive?.State == Card.CardState.Active)
            {
                var item = Instantiate(passiveIndicatorPrefab);
                item.Passive = commander.Passive.Passive;
                item.transform.SetParent(passivePanel);
                hasItems = true;
            }
        }
        passivePanel.gameObject.SetActive(hasItems);

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
        cardsToPlay.Sort((x, y) => x.Speed.CompareTo(y.Speed));
        RoundPointsSpent = 0;
        costText.text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
        confirm.interactable = false;

        CardsLeft.text = cardsToPlay.Count.ToString();
        CardsPlayed.text = cardsPlayed.Count.ToString();
        CardsRemoved.text = cardsDestroyed.Count.ToString();

        confirm.onClick.RemoveAllListeners();
        confirm.onClick.AddListener(() =>
        {
            Hide();
            GameObject.FindObjectOfType<TurnPlate>().SetPlayerReady(player, selection.ToArray());
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
