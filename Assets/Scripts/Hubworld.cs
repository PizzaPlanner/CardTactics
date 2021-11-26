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
        public bool Playable => true;
    }

    [SerializeField] MapWithRequirements[] Maps;

    public void PlayMap(int i)
    {
        if (Maps.Length <= i || !Maps[i].Playable) return;
        var prefab = Maps[i].MapPrefab;
        FindObjectOfType<RoomNetworkManager>().PlayMap(prefab);
    }
}