using Mirror;
using UnityEngine;

public class MapAtlas : MonoBehaviour
{
    [SerializeField] Map[] Maps;
    public Map Prefab(int i) => Maps[i];
    public bool Has(Map map, out int index)
    {
        index = -1;
        for (int i = 0; i < Maps.Length; i++)
            if (Maps[i] == map)
            {
                index = i;
                return true;
            }
        return false;
    }

    void Start()
    {
        foreach (var map in Maps)
            NetworkClient.RegisterPrefab(map.gameObject);
    }
}