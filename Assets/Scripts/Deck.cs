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

    public int CurrentSpeed
    {
        get
        {
            return Commander.Speed + Equipped.Sum(x => x.Speed) + Queued.Sum(x => x.Speed);
        }
    }

    public bool QueueIsValid
    {
        get
        {
            var total = GetTotalCapacities();
            var used = GetUsedCapacities();
            foreach (var cap in used.Keys.ToArray())
            {
                total[cap] -= used[cap];
            }

            return total.Values.All(x => x >= 0) && used.Values.Sum() > 0;
        }
    }

    public Dictionary<CardClass, int> GetUsedCapacities()
    {
        var capacities = new Dictionary<CardClass, int>();

        // Initialize
        foreach (var card in Cards)
        {
            capacities[card.Class] = 0;
            if (card is ActionCard acard)
                foreach (var cost in acard.Cost)
                    capacities[cost.Class] = 0;
            if (card is ItemCard icard)
                capacities[icard.ItemClass] = 0;
        }

        foreach (Card card in Cards)
        {
            if (card is not ActionCard acard || card.State != Card.CardState.Queued) continue;
            foreach (var cost in acard.Cost)
                capacities[cost.Class] += cost.Cost;
        }

        return capacities;
    }

    public Dictionary<CardClass, int> GetTotalCapacities()
    {
        var capacities = new Dictionary<CardClass, int>();

        // Initialize
        foreach (var card in Cards)
        {
            capacities[card.Class] = 0;
            if (card is ActionCard acard)
                foreach (var cost in acard.Cost)
                    capacities[cost.Class] = 0;
            if (card is ItemCard icard)
                capacities[icard.ItemClass] = 0;
        }

        // Count
        foreach (var cap in Commander.Capacities)
            capacities[cap.Class] = cap.Count;
        foreach (var card in Equipped)
            capacities[card.ItemClass] += card.actions;

        // Apply buffs last to handle negative values properly
        foreach (var card in Cards)
        {
            if (card.State != Card.CardState.Active) continue;
            foreach (var skill in card.Skills)
                if (skill is CostModifierPassive p)
                    capacities[p.Buff.Class] = Mathf.Max(0, capacities[p.Buff.Class] + p.Buff.Cost);
        }

        return capacities;
    }

    public IEnumerable<ItemCard> Equipped => Cards.Where(x => x is ItemCard && x.State == Card.CardState.Active).Cast<ItemCard>();
    public IEnumerable<Card> Unplayed => Cards.Where(x => x.State == Card.CardState.Unplayed);
    public IEnumerable<Card> Queued => Cards.Where(x => x.State == Card.CardState.Queued);

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, card is ItemCard ? Card.CardState.Active : Card.CardState.Skipped);
        deck.Commander = deck.ExtendWith(prefab.Commander, null, Card.CardState.Queued);
        deck.Commander.UnitReference.Type = UnitFilter.Commander;
        return deck;
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