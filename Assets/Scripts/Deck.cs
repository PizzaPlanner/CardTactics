using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    public Unit commanderPrefab;
    public Card[] Cards;

    public Unit Create()
    {
        var unit = Card.Instantiate(commanderPrefab);
        var deck = unit.gameObject.AddComponent<IngameDeck>();
        deck.Contexts = new List<CardContext>();
        foreach (var card in Cards)
            deck.Contexts.Add(new CardContext(card, unit, CardState.Unplayed));
        return unit;
    }
}

public static class CardHelper
{
    public static void WriteDeck(this NetworkWriter writer, Deck value)
    {
        writer.WriteString(value == null ? "NULL" : value.name);
    }
    public static Deck ReadDeck(this NetworkReader reader)
    {
        var name = reader.ReadString();
        Debug.Log("Read Deck " + name);
        return Resources.Load<Deck>(name);
    }
}