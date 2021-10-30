using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    // const int POINTS_PER_ROUND_BASE = 8;
    // public int RoundPointsFree { get; private set; }
    public Card[] Cards;

    public UnitCard Commander;

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

            return total.Values.All(x => x >= 0) && Queued.Count() > 0;
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
            capacities[cap.Class] = cap.Cost;
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
    public IEnumerable<ActionCard> Playable
    {
        get
        {
            var used = GetUsedCapacities();
            var total = GetTotalCapacities();
            return Unplayed.Where(x => x is ActionCard acard && acard.Cost.All(x => used[x.Class] + x.Cost <= total[x.Class])).Cast<ActionCard>();
        }
    }

    public void OnPlanningStart(Map map, PlayerInfo player)
    {
        foreach (var card in Cards)
            card.OnPlanningStart(map, player);
    }

    public void OnPlanningEnd(Map map, PlayerInfo player)
    {
        foreach (var card in Cards)
            card.OnPlanningEnd(map, player);
    }

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, card is ItemCard ? Card.CardState.Active : Card.CardState.Unplayed);
        deck.Commander = deck.ExtendWith(prefab.Commander, null, Card.CardState.Unplayed);
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

#if UNITY_EDITOR
    [BoxGroup("Editor")]
    [ShowInInspector] Card editorAddCardPrefab;

    [BoxGroup("Editor")]
    [Button("Add Queued Card")]
    void EditorAddCard()
    {
        ExtendWith(editorAddCardPrefab, null, Card.CardState.Queued);
    }
#endif
}