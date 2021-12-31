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
        Map,
        Hub
    }
    public LoadType Type;

    public void Apply()
    {
        if (Type == LoadType.Map)
            GameObject.Instantiate(MapAtlas.Instance.Item(MapId));
        else if (Type == LoadType.Hub)
            GameObject.Instantiate(HubAtlas.Instance.Item(MapId));
    }
}

public struct LoadingCompleteMessage : NetworkMessage { }

public static class LoadingScreen
{

    public static SceneInfo SceneInfo;

    public static void LoadHub(Hubworld hubPrefab)
    {
        if (!HubAtlas.Instance.TryGet(hubPrefab, out var idx))
        {
            Debug.LogError($"Hub {hubPrefab.name} is not registered!");
            return;
        }

        NetworkServer.SendToAll(new SceneInfo { MapId = idx, Type = SceneInfo.LoadType.Hub });
        RoomNetworkManager.OnSceneChangedServer = () =>
        {
            Debug.Log("Loaded Hub on Server");
            var map = GameObject.Instantiate(hubPrefab);
        };
        NetworkManager.singleton.ServerChangeScene("Empty");
    }

    public static void GoToMenu()
    {
        NetworkManager.singleton.ServerChangeScene("Menu");
    }

    public static void PlayMap(Map mapPrefab)
    {
        if (!MapAtlas.Instance.TryGet(mapPrefab, out var idx))
        {
            Debug.LogError($"Map {mapPrefab.name} is not registered!");
            return;
        }

        var numPlayers = JourneyData.Current.SelectedDecks.Count();

        if (numPlayers < mapPrefab.MinPartySize || numPlayers > mapPrefab.MaxPartySize)
        {
            Debug.LogError($"Can not start this map with Party Size {numPlayers}. Required: {mapPrefab.MinPartySize} to {mapPrefab.MaxPartySize}");
            return;
        }

        NetworkServer.SendToAll(new SceneInfo { MapId = idx, Type = SceneInfo.LoadType.Map });
        RoomNetworkManager.OnSceneChangedServer = () =>
        {
            Debug.Log("Loaded Map on Server");
            var map = GameObject.Instantiate(mapPrefab);
        };
        NetworkManager.singleton.ServerChangeScene("Board");
    }
}
