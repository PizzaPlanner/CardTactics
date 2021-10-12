using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    public PlayerInfo Player;
    [SerializeField] Text Name, CardsLeft, CardsPlayed, CardsRemoved;

    [SerializeField] RectTransform readyBadge;
    [SerializeField] RectTransform passivePanel;
    [SerializeField] CardDisplay activeCardPrefab;
    [SerializeField] CardDisplay commanderCard;
    public Button confirmButton;

    public bool IsReady
    {
        get => ready; set
        {
            readyBadge.gameObject.SetActive(!Player.IsAI && value);
            ready = value;
        }
    }
    bool ready;

    public void UpdateValues()
    {
        Name.color = Player.Color;
        Name.text = Player.Deck.Commander.Name;
        commanderCard.SetCard(Player.Deck.Commander);
        commanderCard.GetComponent<Button>().interactable = false;

        foreach (Transform t in passivePanel)
            GameObject.Destroy(t.gameObject);

        var cardsToPlay = new List<Card>();
        var cardsPlayed = new List<Card>();
        var cardsDestroyed = new List<Card>();
        bool hasPassives = false;
        foreach (var card in Player.Deck.Cards)
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

            if (card.State == Card.CardState.Active && card != Player.Deck.Commander)
            {
                var item = Instantiate(activeCardPrefab);
                item.SetCard(card);
                item.transform.SetParent(passivePanel);
                hasPassives = true;
            }
        }
        CardsLeft.text = cardsToPlay.Count.ToString();
        CardsPlayed.text = cardsPlayed.Count.ToString();
        CardsRemoved.text = cardsDestroyed.Count.ToString();
        // confirmButton.GetComponentInChildren<Text>().text = Player.Deck.RoundPointsFree.ToString();

        passivePanel.gameObject.SetActive(hasPassives);
    }
}