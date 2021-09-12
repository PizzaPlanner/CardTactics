using System;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [System.Serializable]
    public class PlayerInfo
    {
        public Deck Deck;
        public int Team;
        public bool IsAI;
        public Color Color;
    }

    public Map Map;

    public PlayerInfo[] Players;

    public static GameSettings Create(GameSettings prefab)
    {
        var settings = GameSettings.Instantiate(prefab);
        settings.Map = prefab.Map;
        settings.Players = new PlayerInfo[prefab.Players.Length];
        for (int i = 0; i < prefab.Players.Length; i++)
        {
            settings.Players[i] = new PlayerInfo
            {
                Deck = Deck.Create(prefab.Players[i].Deck),
                Color = prefab.Players[i].Color,
                Team = prefab.Players[i].Team,
                IsAI = prefab.Players[i].IsAI
            };
        }

        return settings;
    }
}