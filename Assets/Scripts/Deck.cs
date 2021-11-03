using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    public Card[] Cards;

    public UnitCard Commander;

    List<Card> Played;

    public IEnumerable<ItemCard> Equipped => Cards.Where(x => x is ItemCard && x.State == Card.CardState.Active).Cast<ItemCard>();
    public IEnumerable<Card> Unplayed => Cards.Where(x => x.State == Card.CardState.Unplayed && !Played.Contains(x));
    public IEnumerable<Card> Queued => Cards.Where(x => x.State == Card.CardState.Queued);
    public IEnumerable<ActionCard> Playable(Unit currentUnit) => Unplayed.Where(x => x is ActionCard acard && currentUnit.Resources.CanAfford(acard.Cost)).Cast<ActionCard>();

    public void MarkPlayed(Card card) => Played.Add(card);

    public void OnTurnStart(Map map, PlayerInfo player)
    {
        Played.Clear();
        foreach (var card in Cards)
            card.OnTurnStart(map, player);
    }

    public void OnTurnEnd(Map map, PlayerInfo player)
    {
        foreach (var card in Cards)
            card.OnTurnEnd(map, player);
    }

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, card is ItemCard ? Card.CardState.Active : Card.CardState.Unplayed);
        deck.Commander = deck.ExtendWith(prefab.Commander, null, Card.CardState.Unplayed);
        deck.Commander.UnitReference.Type = UnitFilter.Commander;
        deck.Played = new List<Card>();
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