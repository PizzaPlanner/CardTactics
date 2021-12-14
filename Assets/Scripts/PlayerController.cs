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
        // StartCoroutine to run this in parallel with other animations
        var tp = TurnPanel.FindObjectOfType<TurnPanel>();
        tp.StartCoroutine(tp.OnTurnStart(Unit, null));
        yield return null;
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


public class PlanContext : IPlayableContextWithUnit
{
    public bool Queueable => true;
    public string Name => $"Planning of {Unit.Name}";
    public Unit Unit { get; set; }

    public PlanContext(Unit unit) => Unit = unit;
    public PlanContext() { }

    public IEnumerator PlayOnClient()
    {
        var localOwner = PlayerController.Local.ControlledUnits.Contains(Unit);

        GameObject.FindObjectOfType<PlanningPanel>(true).Show(Unit,
        onPlay: localOwner ? ((cardId) => PlayerController.Local.SetPlayerReady(cardId)) : null,
        onEndTurn: localOwner ? (() => PlayerController.Local.EndTurn()) : null);

        yield return GameSettings.Instance.PlanningPanelPopupTime.Yield();
    }

    public IEnumerator PlayOnServer()
    {
        // TODO: Remove this Method here
        throw new System.NotImplementedException();
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
    public static PlayerController Local;

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
        NetworkClient.RegisterHandler<TilePatternMessage>(OnGetPatterns);
        NetworkClient.RegisterHandler<PlayableMessage>(OnGetPlayable);
        Local = this;
    }

    void OnGetPlayable(PlayableMessage msg)
    {
        Playables.Enqueue(msg.context);
    }

    void OnGetPatterns(TilePatternMessage msg)
    {
        FindObjectOfType<Map>().ShowPatterns(msg.Patterns, true);
    }

    [Command]
    public void SetPlayerReady(int cardId)
    {
        var unit = FindObjectOfType<GameStateMachine>().CurrentUnit;
        FindObjectOfType<Board>().SetPlayerReady(unit.GetComponent<IngameDeck>().Contexts[cardId]);
    }

    [Command]
    public void EndTurn()
    {
        FindObjectOfType<Board>().SetPlayerDone();
    }
}