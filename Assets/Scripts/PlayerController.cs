using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public List<Unit> ControlledUnits;

    [SyncVar] public Deck Deck;

    [Command(requiresAuthority = true)]
    public void SelectDeck(Deck deck)
    {
        Deck = Instantiate(deck);
    }

    public PlayableQueue Playables { get; } = new(false);
    bool playing = false;

    void Update()
    {
        if (Playables.Empty) return;

        var next = Playables.Next();
        IEnumerator PlayAndResume()
        {
            Debug.Log($"[Player] Play {next.Name} for {next.Unit.Name}");
            yield return next.Play(true, false);
        }
        StartCoroutine(PlayAndResume());
    }

    [ClientRpc]
    void OnLoadMap(int mapId)
    {
        var prefab = FindObjectOfType<MapAtlas>().Prefab(mapId);
        Instantiate(prefab);
    }

    [Command]
    public void CreateGame(int i, int numPlayers)
    {
        var mapPrefab = FindObjectOfType<MapAtlas>().Prefab(i);

        if (numPlayers < mapPrefab.MinPartySize || numPlayers > mapPrefab.MaxPartySize)
        {
            Debug.LogError($"Can not start this map with Party Size {numPlayers}. Required: {mapPrefab.MinPartySize} to {mapPrefab.MaxPartySize}");
            return;
        }

        RoomNetworkManager.OnSceneChanged = () =>
        {
            var map = Instantiate(mapPrefab);
            NetworkServer.Spawn(map.gameObject);
            GameObject.FindObjectOfType<Board>().Initialize(map);
            GameObject.FindObjectOfType<GameStateMachine>().StartGame();
            foreach (var controller in FindObjectsOfType<PlayerController>())
                controller.OnLoadMap(i);
        };
        NetworkManager.singleton.ServerChangeScene(LoadingScreen.SCENE_BOARD);
    }

    [Command]
    public void LoadHub(Hubworld hubPrefab)
    {
        LoadingScreen.ChangeSceneServer(
        LoadingScreen.SCENE_EMPTY,
        () =>
        {
            var hub = Instantiate(hubPrefab);
        }, null);
    }

    void Start()
    {
        NetworkClient.RegisterHandler<RoundStartMessage>(OnRoundStart);
        NetworkClient.RegisterHandler<TurnStartMessage>(OnTurnStart);
        NetworkClient.RegisterHandler<StartPlanningMessage>(OnStartPlanning);
    }

    void OnRoundStart(RoundStartMessage msg)
    {
        Debug.Log("Running OnRoundStart");
        StartCoroutine(FindObjectOfType<TurnPanel>().OnRoundStart(msg.round, onComplete: null));
    }

    void OnTurnStart(TurnStartMessage msg)
    {
        Debug.Log("Running OnTurnStart");
        Debug.Assert(msg.netId != null);
        var unit = msg.netId.GetComponent<Unit>();
        Debug.Assert(unit != null);
        StartCoroutine(FindObjectOfType<TurnPanel>().OnTurnStart(unit, onComplete: null));
    }

    void OnStartPlanning(StartPlanningMessage msg)
    {
        Debug.Log("Running OnStartPlanning with netId " + msg.netId);
        var unit = msg.netId.GetComponent<Unit>();
        Debug.Assert(unit != null);
        if (!ControlledUnits.Contains(unit)) return;
        GameObject.FindObjectOfType<PlanningPanel>(true).Show(unit,
        onPlay: (card) => SetPlayerReady(),
        onEndTurn: () => EndTurn());
    }

    [Command]
    void SetPlayerReady()
    {

    }

    [Command]
    void EndTurn()
    {

    }
}

public struct StartPlanningMessage : NetworkMessage
{
    public bool MakeSureSignatureIsUnique;
    public NetworkIdentity netId;
}

public struct RoundStartMessage : NetworkMessage
{
    public int round;
}

public struct TurnStartMessage : NetworkMessage
{
    public NetworkIdentity netId;
}