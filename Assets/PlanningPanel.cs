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
    [SerializeField] CardDisplay cardPrefab;

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
            if (card is PassiveCard pcard && pcard.State == Card.CardState.Active)
            {
                var item = Instantiate(passiveIndicatorPrefab);
                item.Passive = pcard.Passive;
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

        foreach (var card in cardsToPlay)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(cardsPanel);
            visual.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selection.Contains(card))
                {
                    selection.Remove(card);
                    RoundPointsSpent -= card.Cost;
                }
                else
                {
                    RoundPointsSpent += card.Cost;
                    selection.Add(card);
                }
                visual.GetComponent<Button>().animator.SetBool("IsActive", selection.Contains(card));
                confirm.interactable = selection.Count != 0 && RoundPointsSpent <= deck.RoundPointsFree;
                costText.text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
                foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardDisplay>(false))
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
