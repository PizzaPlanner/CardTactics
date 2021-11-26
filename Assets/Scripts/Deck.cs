using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    public Unit commanderPrefab;
    public Card[] Cards;

    public Unit Create(Map map)
    {
        var unit = Card.Instantiate(commanderPrefab);
        var deck = unit.gameObject.AddComponent<IngameDeck>();
        deck.Contexts = new List<CardContext>();
        unit.gameObject.SetActive(false);
        foreach (var card in Cards)
            deck.Contexts.Add(new CardContext(card, map, unit, CardState.Unplayed));
        return unit;
    }
}