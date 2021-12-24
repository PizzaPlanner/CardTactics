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
        unit.GetComponent<IngameDeck>().Contexts.AddRange(Cards.Select(x => new CardContext(x, unit, CardState.Unplayed)));
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