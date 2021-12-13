using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public struct SelectTileMessage : NetworkMessage
{
    public Vector3Int Position;
}

public class Map : NetworkBehaviour
{
    public int MinPartySize => Teams.Sum(x => x.MinPlayers);
    public int MaxPartySize => Teams.Sum(x => x.MaxPlayers);

    List<Tile> Tiles;
    public IEnumerable<Tile> VisibleTiles => Tiles.Where(x => x.gameObject.activeSelf);

    public Team[] Teams;

    public void AddTriggers(Board board)
    {
        foreach (var tcomp in GetComponentsInChildren<TriggerComponent>())
        {
            if (tcomp.TryGetComponent<Tile>(out var tile))
            {
                var ctx = new TriggerContext(tile.Unit, tcomp.Trigger);
                if (ctx.Queueable)
                    board.Playables.Enqueue(ctx);
            }
        }

        foreach (var team in Teams)
        {
            foreach (var condition in team.Conditions.ToArray())
                if (condition.Check(this, team))
                {
                    var firstUnit = FindObjectsOfType<Unit>(true).FirstOrDefault(x => !x.IsDead && Teams[x.Player.Team] == team);
                    if (firstUnit == null)
                        Debug.Log("Team is dead!");
                    team.Conditions.Remove(condition);
                    board.Playables.InsertFirst(new TriggerContext(firstUnit, condition.OnConditionMet));
                }
        }
    }

    public void ShowMarker(int id)
    {
        foreach (var marker in GetComponentsInChildren<Marker>())
        {
            if (marker.Id == id)
                marker.GetComponent<Tile>().SetMarker(true);
        }
    }

    public void HideMarker(int id)
    {
        foreach (var marker in GetComponentsInChildren<Marker>())
        {
            if (marker.Id == id)
                marker.GetComponent<Tile>().SetMarker(false);
        }
    }

    IEnumerable<T> Get<T>(int x, int y, int zMin, int zMax) => GetComponentsInChildren<GridPosition>()
        .Where(gp => gp.Position.x == x && gp.Position.y == y && gp.Position.z >= zMin && gp.Position.z <= zMax && gp.GetComponent<T>() != null)
        .Select(gp => gp.GetComponent<T>());

    void Start()
    {
        var allGridThings = GetComponentsInChildren<GridPosition>(true);
        Tiles = new List<Tile>();
        foreach (var thing in allGridThings)
        {
            if (!thing.TryGetComponent<Tile>(out var tile)) continue;
            Tiles.Add(tile);
            var pos = thing.Position;
            foreach (var neighbour in Get<Tile>(pos.x - 1, pos.y, pos.z - 1, pos.z + 1).Union(Get<Tile>(pos.x, pos.y - 1, pos.z - 1, pos.z + 1)))
            {
                if (Mathf.Abs(thing.Position.z - neighbour.GetComponent<GridPosition>().Position.z) >= 2) continue;

                if (neighbour.Neighbours == null)
                    neighbour.Neighbours = new();
                if (tile.Neighbours == null)
                    tile.Neighbours = new();

                neighbour.Neighbours.Add(tile);
                tile.Neighbours.Add(neighbour);
            }

            if (isServer)
            {
                if (tile.TryGetComponent<UnitSpawn>(out var uspawn))
                {
                    var instance = Instantiate(uspawn.Prefab);
                    instance.Player.Team = -1;
                    Debug.Log("[Board] Spawning prop " + instance.Name);
                    tile.Place(instance, uspawn.Facing);
                }
                else if (tile.TryGetComponent<SpawnLocation>(out var spawn) && tile.TryGetComponent<Owner>(out var owner))
                {
                    Unit instance = Teams[owner.Id].SpawnNew(this);
                    if (instance != null)
                    {
                        Debug.Log("[Board] Spawning unit " + instance.Name);
                        tile.Place(instance, spawn.Facing);
                    }

                }
            }
        }
        foreach (var tile in Tiles)
            if (tile.GetComponent<Area>() != null)
                tile.gameObject.SetActive(false);
        if (isClient)
            Camera.main.transform.position = Camera.main.transform.forward * -50;
        if (isServer)
        {
            foreach (var team in Teams)
                team.CreatePlayables(FindObjectOfType<Board>().Playables, this);
            NetworkServer.RegisterHandler<SelectTileMessage>(OnSelectTileMessage);
        }
    }

    void OnSelectTileMessage(NetworkConnection conn, SelectTileMessage message)
    {
        var tile = Tiles.FirstOrDefault(x => x.Position == message.Position);
        if (tile == null)
            return;
        if (tile.PrimaryPattern != null)
        {
            Debug.Log("Selected Tile " + tile.Position);
            OnTileSelection?.Invoke(tile.PrimaryPattern);
        }
    }

    public void RevealArea(int areaId)
    {
        foreach (var tile in Tiles)
            if (tile.TryGetComponent<Area>(out var area) && area.Id == areaId)
                tile.gameObject.SetActive(true);
    }

    internal Tile TileOf(Unit unit)
    {
        return Tiles.FirstOrDefault(x => x.Unit == unit);
    }

    public Action<ITilePattern> OnTileSelection;

    [Server]
    public void ShowTileIndicators(IEnumerable<ITilePattern> patterns, Func<ITilePattern, TargetIndicator> indicatorFunc, Action<ITilePattern> onSelectPattern = null)
    {
        OnTileSelection = onSelectPattern;
        foreach (var tile in Tiles)
        {
            tile.CurrentIndicator = TargetIndicator.Hidden;
            foreach (var pattern in patterns)
            {
                if (pattern.Tiles.Contains(tile))
                {
                    if (pattern.UnselectableTiles.Contains(tile))
                        continue;
                    if (tile.PrimaryPattern != null)
                        break;

                    tile.PrimaryPattern = pattern;
                    tile.CurrentIndicator = indicatorFunc(pattern);
                }
            }
        }
        NetworkServer.SendToAll(new TilePatternMessage { Patterns = patterns.ToArray() });
    }

    [Client]
    public void ShowPatterns(IEnumerable<ITilePattern> patterns, bool hasAuthority)
    {
        foreach (var tile in Tiles)
        {
            // TODO: This is a workaround for Host mode
            if (isServer)
                tile.PrimaryPattern = null;
            tile.CurrentIndicator = TargetIndicator.Hidden;
            foreach (var pattern in patterns)
            {
                if (pattern.Tiles.Contains(tile))
                {
                    if (pattern.UnselectableTiles.Contains(tile))
                        continue;
                    if (tile.PrimaryPattern != null)
                        break;

                    tile.PrimaryPattern = pattern;
                    tile.CurrentIndicator = hasAuthority ? TargetIndicator.Valid : TargetIndicator.Preview;

                    tile.SetIndicatorVisible(true, pattern.Tiles.Contains(Tile.Hovered));
                }
            }
        }
    }

    public Tile[] GetSpawnTiles(SelectionFilter filter, int team)
    {
        List<Tile> valid = new List<Tile>();
        foreach (var owner in GetComponentsInChildren<Owner>())
        {
            if (owner.Id != team || !owner.TryGetComponent<SpawnLocation>(out var _)) continue;
            var tile = owner.GetComponent<Tile>();
            if (TileMatches(tile, filter, null))
                valid.Add(tile);
        }
        return valid.ToArray();
    }

    public void HideTileSelectionPreview()
    {
        OnTileSelection = null;
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            tile.CurrentIndicator = TargetIndicator.Hidden;
            tile.ClearPathingInfo();
            tile.SetIndicatorVisible(false, false);
            tile.PrimaryPattern = null;
        }
    }

    public bool TileMatches(Tile tile, SelectionFilter filter, Unit source)
    {
        if (!tile.gameObject.activeSelf)
            return false;
        var validTarget = (((filter.Tile & TileFilter.Empty) > 0) && (tile.Unit == null)) || filter.Tile == 0;
        if (filter.Unit > 0 && tile.Unit != null)
        {
            var isObstacle = tile.Unit.Player.Team == -1;
            validTarget |= ((filter.Tile & TileFilter.Obstacle) > 0) && isObstacle;
            validTarget |= ((filter.Tile & TileFilter.Self) > 0) && !isObstacle && tile.Unit == source;
            validTarget |= ((filter.Tile & TileFilter.Ally) > 0) && !isObstacle && tile.Unit.Player.Team == source.Player.Team;
            validTarget |= ((filter.Tile & TileFilter.Enemy) > 0) && !isObstacle && tile.Unit.Player.Team != source.Player.Team;

            validTarget &= (filter.Unit & tile.Unit.Type) > 0;
        }
        return validTarget;
    }

    void Update()
    {
        if (!isClient)
            return;

        var currentTile = Tile.Hovered;
        Tile newTile = null;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Tile")))
            newTile = hit.transform.GetComponent<Tile>();
        if (newTile != currentTile && currentTile != null)
            currentTile.OnMouseLeave();
        if (newTile != null)
            newTile.OnMouseEnter();

        if (GameStateMachine.WaitingForAction)
            switch (GameStateMachine.CurrentStage)
            {
                case GameStateMachine.Stage.Plan:
                case GameStateMachine.Stage.Play:
                    DoControls();
                    break;
            }
    }

    [Client]
    void DoControls()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
            Camera.main.orthographicSize += scroll;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.one * 0.5f + Vector3.down * 0.5f, new Vector3(1, 0, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.one * 0.5f + Vector3.down * 0.5f, new Vector3(0, 0, 1));
    }
#endif
}