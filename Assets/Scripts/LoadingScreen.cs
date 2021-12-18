using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public struct SceneInfo : NetworkMessage
{
    public int MapId;
    public enum LoadType
    {
        None,
        Map
    }
    public LoadType Type;

    public void Apply()
    {
        if (Type == LoadType.Map)
            GameObject.Instantiate(GameObject.FindObjectOfType<MapAtlas>().Prefab(MapId));
    }
}

public static class LoadingScreen
{

    public static SceneInfo SceneInfo;

    public static void LoadHub(Hubworld hubPrefab)
    {
    }

    public static void GoToHub(Hubworld hub)
    {
    }

    public static void GoToMenu()
    {
        NetworkManager.singleton.ServerChangeScene("Menu");
    }

    public static void PlayMap(Map mapPrefab)
    {
        if (!GameObject.FindObjectOfType<MapAtlas>().Has(mapPrefab, out var idx))
        {
            Debug.LogError($"Map {mapPrefab.name} is not registered!");
            return;
        }

        CreateGame(idx, NetworkManager.singleton.numPlayers);
    }


    static void CreateGame(int i, int numPlayers)
    {
        var mapPrefab = MapAtlas.FindObjectOfType<MapAtlas>().Prefab(i);

        if (numPlayers < mapPrefab.MinPartySize || numPlayers > mapPrefab.MaxPartySize)
        {
            Debug.LogError($"Can not start this map with Party Size {numPlayers}. Required: {mapPrefab.MinPartySize} to {mapPrefab.MaxPartySize}");
            return;
        }

        NetworkServer.SendToAll(new SceneInfo { MapId = i, Type = SceneInfo.LoadType.Map });
        RoomNetworkManager.OnSceneChangedServer = () =>
        {
            IEnumerator coroutine()
            {
                Debug.Log("Waiting for all players to be ready...");
                yield return new WaitUntil(() => NetworkServer.connections.Values.All(x => x.isReady));
                Debug.Log("Loading Map on Server");
                var map = GameObject.Instantiate(mapPrefab);
            }
            RoomNetworkManager.singleton.StartCoroutine(coroutine());
        };
        NetworkManager.singleton.ServerChangeScene("Board");
    }

}
