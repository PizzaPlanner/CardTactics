using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    [SerializeField] Card[] Cards;

    public UnitCard Commander => Cards.First(x => x is UnitCard) as UnitCard;

    public IEnumerable<Card> All => Cards;
    public IEnumerable<ItemCard> Equipped => Cards.Where(x => x is ItemCard && x.State == Card.CardState.Active).Cast<ItemCard>();
    public IEnumerable<Card> Unplayed => Cards.Where(x => x.State == Card.CardState.Unplayed);
    public IEnumerable<ActionCard> Playable(Unit currentUnit) => Unplayed.Where(x => x is ActionCard acard && currentUnit.Resources.CanAfford(acard.Cost)).Cast<ActionCard>();

    public void OnRoundStart(Map map, PlayerInfo player)
    {
        foreach (var card in Cards)
            card.OnRoundStart(map, player);
    }

    public void OnRoundEnd(Map map, PlayerInfo player)
    {
        foreach (var card in Cards)
            card.OnRoundEnd(map, player);
    }

    public static Deck Create(Deck prefab)
    {
        var deck = Deck.Instantiate(prefab);
        deck.Cards = new Card[0];
        foreach (var card in prefab.Cards)
            deck.ExtendWith(card, null, card is ItemCard ? Card.CardState.Active : Card.CardState.Unplayed);

        Assert.IsNotNull(deck.Commander);
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
        ExtendWith(editorAddCardPrefab, null, Card.CardState.Unplayed);
    }
#endif
}