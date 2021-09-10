using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public int Player;
    public Card[] Cards;

    public RectTransform PanelPrefab;

    public RectTransform SpellCardPrefab;

    RectTransform panel;
    RectTransform cardsPanel;
    Button confirm;
    public void Initialize()
    {
        panel = Instantiate(PanelPrefab);
        cardsPanel = panel.GetChild(0) as RectTransform;
        confirm = panel.GetComponentInChildren<Button>();
        panel.SetParent(FindObjectOfType<Canvas>().transform);
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(1, 0);
        panel.anchoredPosition = new Vector2(0.5f, 0);
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
        confirm.interactable = false;
        confirm.onClick.RemoveAllListeners();
        confirm.onClick.AddListener(() =>
        {
            GameObject.FindObjectOfType<TurnPlate>().SetPlayerReady(Player, selection.ToArray());
            Hide();
        });
        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (var cardPrefab in Cards)
        {
            var card = cardPrefab.FullCard();
            card.SetParent(cardsPanel);
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selection.Contains(cardPrefab))
                    selection.Remove(cardPrefab);
                else
                    selection.Add(cardPrefab);
                card.GetComponent<Button>().animator.SetBool("IsActive", selection.Contains(cardPrefab));
                confirm.interactable = selection.Count != 0;
            });
        }
    }
}