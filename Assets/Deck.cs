using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    [System.Serializable]
    public struct Capacity
    {
        public CardClass Class;
        public int MaxNumber;
    }

    const int POINTS_PER_ROUND_BASE = 8;
    public int RoundPointsFree { get; private set; }
    public Card[] Cards;

    public UnitCard Commander;

    public bool Validate()
    {
        foreach (var capacity in Commander.Capacities)
        {
            if (Cards.Count(x => x.Class == capacity.Class) > capacity.MaxNumber)
                return false;
        }
        return true;
    }

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.RoundPointsFree = POINTS_PER_ROUND_BASE;
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, Card.CardState.Skipped);
        deck.Commander = deck.ExtendWith(prefab.Commander, null, Card.CardState.Unplayed);
        deck.Commander.UnitReference.Type = UnitFilter.Commander;
        return deck;
    }

    public void ShuffleIfNeccessary()
    {
        foreach (var card in Cards)
        {
            if (card.State == Card.CardState.Unplayed)
                return;
        }

        Shuffle();
    }

    void Shuffle()
    {
        foreach (var card in Cards)
        {
            if (card.State == Card.CardState.Skipped || card.State == Card.CardState.Played)
                card.Reshuffle();
        }
    }

    public T ExtendWith<T>(T cardPrefab, Card source, Card.CardState initialState) where T : Card
    {
        var newCard = Instantiate(cardPrefab);
        newCard.Init(source, initialState);

        var listC = Cards.ToList();
        listC.Add(newCard);
        Cards = listC.ToArray();

        return newCard;
    }
}