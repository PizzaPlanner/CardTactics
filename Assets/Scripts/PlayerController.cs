using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoundStartContext : IPlayableContext
{
    public bool Queueable => true;
    public string Name => $"Round {Round}";
    public int Round { get; set; }

    public IEnumerator PlayOnClient()
    {
        yield return TurnPanel.FindObjectOfType<TurnPanel>().OnRoundStart(Round, onComplete: null);
    }

    public IEnumerator PlayOnServer()
    {
        NetworkServer.SendToAll(new PlayableMessage { context = this });
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteInt(Round);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Round = reader.ReadInt();
        return this;
    }
}

public class TurnStartContext : IPlayableContextWithUnit
{
    public bool Queueable => true;
    public string Name => $"Turn of {Unit.Name}";
    public Unit Unit { get; set; }

    public TurnStartContext(Unit unit) => Unit = unit;
    public TurnStartContext() { }

    public IEnumerator PlayOnClient()
    {
        yield return TurnPanel.FindObjectOfType<TurnPanel>().OnTurnStart(Unit, onComplete: null);
    }

    public IEnumerator PlayOnServer()
    {
        NetworkServer.SendToAll(new PlayableMessage { context = this });
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }
}

public class PlayerController : NetworkBehaviour
{
    public List<Unit> ControlledUnits;

    [SyncVar] public Deck Deck;

    [Command(requiresAuthority = true)]
    public void SelectDeck(Deck deck)
    {
        Deck = Instantiate(deck);
    }

    public PlayableQueue Playables { get; } = new();

    bool isPlaying;

    void Update()
    {
        if (Playables.Empty || isPlaying) return;

        var next = Playables.Next();
        IEnumerator PlayAndResume()
        {
            if (next is IPlayableContextWithUnit ipcu)
                Debug.Log($"[Playable] Play {next.Name} for {ipcu.Unit.Name} (@{gameObject.name})", ipcu.Unit);
            else
                Debug.Log($"[Playable] Play {next.Name} (@{gameObject.name})");
            yield return next.PlayOnClient();
            isPlaying = false;
        }
        isPlaying = true;
        StartCoroutine(PlayAndResume());
    }

    [ClientRpc]
    void OnLoadMap(int mapId)
    {
        if (isServer) return;
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

        RoomNetworkManager.OnSceneChangedServer = () =>
        {
            var map = Instantiate(mapPrefab);
            NetworkServer.Spawn(map.gameObject);
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

    public override void OnStartLocalPlayer()
    {
        NetworkClient.RegisterHandler<StartPlanningMessage>(OnStartPlanning);
        NetworkClient.RegisterHandler<TilePatternMessage>(OnGetPatterns);
        NetworkClient.RegisterHandler<PlayableMessage>(OnGetPlayable);
    }

    void OnGetPlayable(PlayableMessage msg)
    {
        Playables.Enqueue(msg.context);
    }

    void OnGetPatterns(TilePatternMessage msg)
    {
        FindObjectOfType<Map>().ShowPatterns(msg.Patterns, true);
    }

    void OnStartPlanning(StartPlanningMessage msg)
    {
        var unit = msg.netId.GetComponent<Unit>();
        Debug.Assert(unit != null);
        if (!ControlledUnits.Contains(unit)) return;
        GameObject.FindObjectOfType<PlanningPanel>(true).Show(unit,
        onPlay: ControlledUnits.Contains(unit) ? ((cardId) => SetPlayerReady(unit, cardId)) : null,
        onEndTurn: ControlledUnits.Contains(unit) ? (() => EndTurn()) : null);
    }

    [Command]
    void SetPlayerReady(Unit unit, int cardId)
    {
        FindObjectOfType<Board>().SetPlayerReady(unit.GetComponent<IngameDeck>().Contexts[cardId]);
    }

    [Command]
    void EndTurn()
    {
        FindObjectOfType<Board>().SetPlayerDone();
    }
}

public struct StartPlanningMessage : NetworkMessage
{
    public NetworkIdentity netId;
}