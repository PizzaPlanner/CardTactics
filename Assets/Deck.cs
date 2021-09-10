using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public int RoundPointsFree;
    int RoundPointsSpent;
    public int Player;
    public Card[] Cards;

    List<Card> CardsToPlay;
    List<Card> CardsPlayed;
    List<Card> CardsDestroyed;

    [SerializeField] Text costText;
    [SerializeField] RectTransform panel;
    [SerializeField] RectTransform cardsPanel;
    [SerializeField] Button confirm;

    public void Initialize()
    {
        CardsToPlay = new List<Card>();
        CardsPlayed = new List<Card>();
        CardsDestroyed = new List<Card>();
        foreach (var card in Cards)
        {
            var cardInst = Card.Instantiate(card);
            for (int i = 0; i < cardInst.Skills.Length; i++)
                cardInst.Skills[i] = Skill.Instantiate(cardInst.Skills[i]);
            CardsToPlay.Add(cardInst);
        }
    }

    public void Show()
    {
        panel.gameObject.SetActive(true);
        SetCards();
    }

    public void Hide()
    {
        panel.gameObject.SetActive(false);
    }

    List<Card> selection;

    void SetCards()
    {
        selection = new List<Card>();
        RoundPointsSpent = 0;
        costText.text = $"{RoundPointsSpent}/{RoundPointsFree}";
        confirm.interactable = false;
        if (CardsToPlay.Count == 0)
        {
            CardsToPlay.AddRange(CardsPlayed);
            CardsPlayed.Clear();
        }
        CardsToPlay.Sort((x, y) => x.Speed.CompareTo(y.Speed));
        confirm.onClick.RemoveAllListeners();
        confirm.onClick.AddListener(() =>
        {
            Hide();
            GameObject.FindObjectOfType<TurnPlate>().SetPlayerReady(Player, selection.ToArray());
            foreach (var card in selection)
            {
                CardsToPlay.Remove(card);
                if (card.Once)
                    CardsDestroyed.Add(card);
                else
                    CardsPlayed.Add(card);
            }
        });
        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (var cardPrefab in CardsToPlay)
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
                confirm.interactable = selection.Count != 0 && RoundPointsSpent <= RoundPointsFree;
                costText.text = $"{RoundPointsSpent}/{RoundPointsFree}";
                foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardReference>(false))
                {
                    // if (otherCard == null || otherCard.Reference == null) continue;
                    if (selection.Contains(otherCard.Reference) || RoundPointsSpent + otherCard.Reference.Cost <= RoundPointsFree)
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

        GameObject.FindObjectOfType<PlanningPanel>().UpdateUI(this, CardsToPlay.Count, CardsPlayed.Count, CardsDestroyed.Count);
    }
}