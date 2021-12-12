using UnityEngine;
using Mirror;
using UnityEngine.UIElements;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class RoomNetworkManager : NetworkManager
{
    VisualElement lobbyUI, loadingScreen;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        lobbyUI = GetComponent<UIDocument>().rootVisualElement.Q("Lobby");
        lobbyUI.Q("PlayerList").Add(new Label(conn.identity.gameObject.name));
        lobbyUI.Q<Toggle>().value = conn.isReady;
        lobbyUI.Q<Toggle>().RegisterValueChangedCallback(x => conn.isReady = x.newValue);
        DontDestroyOnLoad(conn.identity.gameObject);
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
    [SerializeField] VisualTreeAsset loadingScreenPrefab;

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

        loadingScreenPrefab.CloneTree(root, out idx, out var _);
        loadingScreen = root[idx].Q("LoadingScreen");
        loadingScreen.style.display = DisplayStyle.None;

        StartHost();
    }

    public override void OnStartClient()
    {
        NetworkClient.ready = true;
        NetworkClient.AddPlayer();
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
    }

    public static Action OnSceneChangedServer;
    public override void OnServerSceneChanged(string newSceneName)
    {
        OnSceneChangedServer?.Invoke();
        OnSceneChangedServer = null;
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        loadingScreen.style.display = DisplayStyle.None;
        lobbyUI.style.display = SceneManager.GetActiveScene().name == "Menu" ? DisplayStyle.Flex : DisplayStyle.None;

    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log("Now loading...");
        loadingScreen.style.display = DisplayStyle.Flex;
    }
}
