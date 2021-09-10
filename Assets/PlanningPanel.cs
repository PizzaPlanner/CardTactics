using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningPanel : MonoBehaviour
{
    [SerializeField] Text Name, CardsLeft, CardsPlayed, CardsRemoved;

    public void UpdateUI(Deck deck, int cardsLeft, int cardsPlayed, int cardsRemoved)
    {
        Name.text = deck.name;
        Name.color = Board.PlayerColor(deck.Player);
        CardsLeft.text = cardsLeft.ToString();
        CardsPlayed.text = cardsPlayed.ToString();
        CardsRemoved.text = cardsRemoved.ToString();
    }
}
