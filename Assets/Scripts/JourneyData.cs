using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JourneyData : ScriptableObject
{
    [System.Serializable]
    public struct PlayerWithDeck
    {
        public int PlayerId;
        public int DeckId;
    }

    public static JourneyData Current { get; private set; }

    public Deck[] AvailableDecks;
    public List<PlayerWithDeck> SelectedDecks;

    public static void PrepareNew(Deck[] decks)
    {
        var inst = JourneyData.CreateInstance<JourneyData>();
        inst.SelectedDecks = new();
        inst.AvailableDecks = decks;
        Current = inst;
    }
}