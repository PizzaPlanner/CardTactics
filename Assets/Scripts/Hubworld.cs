using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public interface IJourneyProvider
{
    Deck[] StartingDecks { get; }
}

public class Hubworld : MonoBehaviour, IJourneyProvider
{
    [System.Serializable]
    class MapWithRequirements
    {
        public Map MapPrefab;
        public string[] RequiredAchievements;
        public string[] PreventingAchievements;
        public bool Playable => true;
    }

    [SerializeField] public int TeamSize;
    [SerializeField] MapWithRequirements[] Maps;
    [SerializeField] public Deck[] AvailableDecks;

    public Deck[] StartingDecks => AvailableDecks;

    public void PlayMap(int i)
    {
        if (Maps.Length <= i || !Maps[i].Playable) return;
        var prefab = Maps[i].MapPrefab;
        LoadingScreen.PlayMap(prefab);
    }
}