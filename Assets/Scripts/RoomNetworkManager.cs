using UnityEngine;
using Mirror;
using UnityEngine.UIElements;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class RoomNetworkManager : NetworkManager
{
    VisualElement lobbyUI, loadingScreen;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("[Server] Player joined");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        SetConnectionUI(NetworkManagerMode.ClientOnly);
        Debug.Log("I joined");
    }

    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<PlayableMessage>((msg) => { if (Playables.Mode == NetworkManagerMode.ClientOnly) Playables.Enqueue(msg.context); });
        NetworkClient.RegisterHandler<SkillPlanContext.CancelContextMessage>((_) => SkillPlanContext.CancelContextMessage.CancelCurrentContext++);
        NetworkClient.RegisterHandler<SceneInfo>((msg) => LoadingScreen.SceneInfo = msg);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        SetConnectionUI(NetworkManagerMode.Offline);
    }

    public override void OnStopClient()
    {
        SetConnectionUI(NetworkManagerMode.Offline);
    }

    public override void OnClientError(Exception exception)
    {
        SetConnectionUI(NetworkManagerMode.Offline);
    }

    public override void OnStartHost()
    {
        SetConnectionUI(NetworkManagerMode.Host);
    }

    void SetConnectionUI(NetworkManagerMode m)
    {
        var players = lobbyUI.Q("PlayerList");
        players.Clear();
        switch (m)
        {
            case NetworkManagerMode.Offline:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.None;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.Flex;
                lobbyUI.Q("Leave").style.display = DisplayStyle.None;
                lobbyUI.Q("Host").style.display = DisplayStyle.Flex;
                break;
            case NetworkManagerMode.ServerOnly:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.Flex;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.None;
                lobbyUI.Q("Leave").style.display = DisplayStyle.None;
                lobbyUI.Q("Host").style.display = DisplayStyle.None;
                break;
            case NetworkManagerMode.ClientOnly:
            case NetworkManagerMode.Host:
                lobbyUI.Q("Connecting").style.display = DisplayStyle.None;
                lobbyUI.Q("JoinPanel").style.display = DisplayStyle.None;
                lobbyUI.Q("Leave").style.display = DisplayStyle.Flex;
                lobbyUI.Q("Host").style.display = DisplayStyle.None;
                foreach (var player in FindObjectsOfType<PlayerController>())
                    players.Add(new Label(player.gameObject.name));
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

        if (SceneManager.GetActiveScene().name == "Entrypoint")
            LoadingScreen.GoToMenu();

        spawnPrefabs.AddRange(UnitAtlas.Instance.All.Select(x => x.gameObject));

        try
        {
            StartHost();
        }
        catch { }
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

    public static Action OnSceneChangedServer;
    public override void OnServerSceneChanged(string newSceneName)
    {
        Playables = new PlayableQueue(mode);
        OnSceneChangedServer?.Invoke();
        OnSceneChangedServer = null;
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        lobbyUI.style.display = SceneManager.GetActiveScene().name == "Menu" ? DisplayStyle.Flex : DisplayStyle.None;

        Debug.Log("Player added");

        if (mode != NetworkManagerMode.Host)
            LoadingScreen.SceneInfo.Apply();
        else
            Debug.Log("Skipped Applying SceneInfo in Host Mode");

        if (mode == NetworkManagerMode.ClientOnly)
            Playables = new PlayableQueue(mode);
        NetworkClient.ready = true;
        NetworkClient.AddPlayer();
        loadingScreen.style.display = DisplayStyle.None;
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log("Now loading...");
        loadingScreen.style.display = DisplayStyle.Flex;
    }

    public static PlayableQueue Playables { get; private set; } = new(NetworkManagerMode.Offline);

    void Update()
    {
        if (Playables.Mode == NetworkManagerMode.ClientOnly && Playables.TryGetNext(out var coroutine))
            StartCoroutine(coroutine);
    }
}
