using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Capacity
{
    public CardClass Class;
    public int Count;
}

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    // const int POINTS_PER_ROUND_BASE = 8;
    // public int RoundPointsFree { get; private set; }
    public Card[] Cards;

    public UnitCard Commander;

    public int ClassesPerTurn => Commander.ClassesPerTurn;

    public Dictionary<CardClass, int> GetCapacities()
    {
        var capacities = new Dictionary<CardClass, int>();
        foreach (var cap in Commander.Capacities)
            capacities[cap.Class] = cap.Count;
        foreach (var card in Cards)
        {
            if (!capacities.ContainsKey(card.Class))
                capacities[card.Class] = 0;
            if (card is ActionCard acard && !capacities.ContainsKey(acard.RequiredClass))
                capacities[acard.RequiredClass] = 0;
            if (card is ItemCard icard && !capacities.ContainsKey(icard.ItemClass))
                capacities[icard.ItemClass] = 0;
        }
        foreach (var card in Cards)
            if (card is ItemCard icard && icard.State == Card.CardState.Active)
                capacities[icard.ItemClass] += icard.actions;

        return capacities;
    }

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        // deck.RoundPointsFree = POINTS_PER_ROUND_BASE;
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, card is ItemCard ? Card.CardState.Active : Card.CardState.Skipped);
        deck.Commander = deck.ExtendWith(prefab.Commander, null, Card.CardState.Unplayed);
        deck.Commander.UnitReference.Type = UnitFilter.Commander;
        return deck;
    }

    public void ShuffleIfNeccessary()
    {
        // foreach (var card in Cards)
        // {
        //     if (card.State == Card.CardState.Unplayed)
        //         return;
        // }

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