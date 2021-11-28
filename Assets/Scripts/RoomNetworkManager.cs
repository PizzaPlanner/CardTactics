using UnityEngine;
using Mirror;
using UnityEngine.UIElements;
using System.Linq;
using System;

[RequireComponent(typeof(UIDocument))]
public class RoomNetworkManager : NetworkManager
{
    VisualElement lobbyUI;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        lobbyUI = GetComponent<UIDocument>().rootVisualElement.Q("Lobby");
        lobbyUI.Q("PlayerList").Add(new Label(conn.identity.gameObject.name));
        lobbyUI.Q<Toggle>().value = conn.isReady;
        lobbyUI.Q<Toggle>().RegisterValueChangedCallback(x => conn.isReady = x.newValue);
        DontDestroyOnLoad(conn.identity);
        SetConnectionUI(NetworkManagerMode.ClientOnly);

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        lobbyUI.Q("PlayerList").Clear();
        SetConnectionUI(NetworkManagerMode.Offline);
    }

    public override void OnStopClient()
    {
        lobbyUI.Q("PlayerList").Clear();
        SetConnectionUI(NetworkManagerMode.Offline);
    }

    public override void OnStartHost()
    {
        SetConnectionUI(NetworkManagerMode.Host);
    }

    public override void OnStartServer()
    {
        Debug.Log("Server started!");
    }

    void SetConnectionUI(NetworkManagerMode m)
    {
        switch (m)
        {
            case NetworkManagerMode.Offline:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.None;
                lobbyUI.Q("Ready").style.display = DisplayStyle.None;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.Flex;
                lobbyUI.Q("Leave").style.display = DisplayStyle.None;
                lobbyUI.Q("Host").style.display = DisplayStyle.Flex;
                break;
            case NetworkManagerMode.ServerOnly:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.Flex;
                lobbyUI.Q("Ready").style.display = DisplayStyle.None;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.None;
                lobbyUI.Q("Leave").style.display = DisplayStyle.None;
                lobbyUI.Q("Host").style.display = DisplayStyle.None;
                break;
            case NetworkManagerMode.ClientOnly:
            case NetworkManagerMode.Host:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.None;
                lobbyUI.Q("Ready").style.display = DisplayStyle.Flex;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.None;
                lobbyUI.Q("Leave").style.display = DisplayStyle.Flex;
                lobbyUI.Q("Host").style.display = DisplayStyle.None;
                break;
        }
    }

    [SerializeField] VisualTreeAsset lobbyUIPrefab;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        lobbyUIPrefab.CloneTree(root, out var idx, out var _);
        lobbyUI = root[idx].Q("Lobby");
        lobbyUI.Q<TextField>("ServerIp").value = networkAddress;
        lobbyUI.Q<TextField>("ServerIp").RegisterValueChangedCallback(x => this.networkAddress = x.newValue);
        lobbyUI.Q<Button>("Join").clicked += Connect;
        lobbyUI.Q<Button>("Leave").clicked += Leave;
        lobbyUI.Q<Button>("Host").clicked += Host;

        StartHost();
    }

    void Host()
    {
        SetConnectionUI(NetworkManagerMode.ServerOnly);
        StartServer();
    }

    void Connect()
    {
        SetConnectionUI(NetworkManagerMode.ServerOnly);
        StartClient();
    }

    void Leave()
    {
        SetConnectionUI(NetworkManagerMode.ServerOnly);
        StopHost();
    }

    public void GoToHub(Hubworld hub)
    {
        FindObjectsOfType<PlayerController>().First(x => x.hasAuthority).LoadHub(hub);
    }

    public void PlayMap(Map mapPrefab)
    {
        if (!FindObjectOfType<MapAtlas>().Has(mapPrefab, out var idx))
        {
            Debug.LogError($"Map {mapPrefab.name} is not registered!");
            return;
        }

        FindObjectsOfType<PlayerController>().First(x => x.hasAuthority).CreateGame(idx, numPlayers);
        GetComponent<UIDocument>().enabled = false;
    }

    public static Action OnSceneChanged;
    public override void OnServerSceneChanged(string newSceneName)
    {
        OnSceneChanged?.Invoke();
        OnSceneChanged = null;

    }
}
