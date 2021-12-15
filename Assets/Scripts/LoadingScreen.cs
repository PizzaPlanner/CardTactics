using Mirror;
using UnityEngine;

public static class LoadingScreen
{
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

        RoomNetworkManager.OnSceneChangedServer = () =>
        {
            var map = GameObject.Instantiate(mapPrefab);
            NetworkServer.Spawn(map.gameObject);
            foreach (var player in GameObject.FindObjectsOfType<PlayerController>())
                player.OnLoadMap(i);
        };
        NetworkManager.singleton.ServerChangeScene("Board");
    }

}
