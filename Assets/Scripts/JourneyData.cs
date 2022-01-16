using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public struct JourneyDataUpdate : NetworkMessage
{
    public Deck[] Decks;
    public JourneyData.PlayerWithDeck[] Players;
}

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

    [Server]
    public static bool PrepareNew(Deck[] decks)
    {
        if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
            return false;
        var inst = JourneyData.CreateInstance<JourneyData>();
        inst.SelectedDecks = new();
        inst.AvailableDecks = decks;
        Current = inst;
        return true;
    }

    [Client]
    public static void UpdateFrom(JourneyDataUpdate update)
    {
        if (Current == null)
            Current = JourneyData.CreateInstance<JourneyData>();
        Current.AvailableDecks = update.Decks;
        Current.SelectedDecks = update.Players.ToList();

    }
}