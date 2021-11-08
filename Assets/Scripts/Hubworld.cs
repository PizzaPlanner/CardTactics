using System.Linq;
using UnityEngine;

public class Hubworld : MonoBehaviour
{
    [System.Serializable]
    class MapWithRequirements
    {
        public Map MapPrefab;
        public string[] RequiredAchievements;
        public string[] PreventingAchievements;

        public bool Playable(Lobby lobby) =>
            RequiredAchievements.Count(x => !lobby.Achievements.Contains(x)) == 0 &&
            PreventingAchievements.Count(x => lobby.Achievements.Contains(x)) == 0;

    }

    [SerializeField] MapWithRequirements[] Maps;

    public void PlayMap(int i)
    {
        var lobby = Lobby.Current;
        if (Maps.Length <= i || !Maps[i].Playable(lobby)) return;
        var prefab = Maps[i].MapPrefab;
        if (lobby.PlayerCount < prefab.MinPartySize || lobby.PlayerCount > prefab.MaxPartySize)
        {
            Debug.LogError("Can not start this map with Party Size " + lobby.PlayerCount);
            return;
        }

        LoadingScreen.CreateGame(prefab, lobby);
    }
}