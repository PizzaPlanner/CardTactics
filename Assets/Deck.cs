using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    const int POINTS_PER_ROUND_BASE = 8;
    public int RoundPointsFree { get; private set; }
    public Card[] Cards;

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.Cards = new Card[prefab.Cards.Length];
        deck.RoundPointsFree = POINTS_PER_ROUND_BASE;
        for (int i = 0; i < prefab.Cards.Length; i++)
        {
            var cardInst = Card.Instantiate(prefab.Cards[i]);
            for (int j = 0; j < cardInst.Skills.Length; j++)
            {
                cardInst.Init();
            }
            deck.Cards[i] = cardInst;
        }
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
            card.Reshuffle();
        }
    }
}