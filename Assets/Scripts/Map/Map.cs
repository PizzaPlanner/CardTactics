using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class Map : MonoBehaviour
{
    [SerializeField] public PlayerInfo[] Players;
    public int MinPartySize => Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired);
    public int MaxPartySize => Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired || x.Slot == PlayerInfo.SlotType.PlayerOptional);

    List<Tile> Tiles;
    public IEnumerable<Tile> VisibleTiles => Tiles.Where(x => x.gameObject.activeSelf);
    List<int> revealedAreas;

    public List<(PlayerInfo player, Trigger trigger)> CheckTriggers()
    {
        var l = new List<(PlayerInfo player, Trigger trigger)>();
        foreach (var tcomp in GetComponentsInChildren<TriggerComponent>())
        {
            if (tcomp.TryGetComponent<Tile>(out var tile) && tcomp.Trigger.ShouldTrigger(this, tile))
                l.Add((tile.Unit?.Player ?? null, tcomp.Trigger));
        }
        return l;
    }

    internal IEnumerable<PlayerInfo> GetSpawnablePlayers()
    {
        foreach (var spawn in GetComponentsInChildren<SpawnLocation>())
            if (spawn.TryGetComponent<Owner>(out var owner) && owner.Id < Players.Length && (!spawn.TryGetComponent<Area>(out var area) || (revealedAreas?.Contains(area.Id) ?? false)))
                yield return Players[owner.Id];
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

    public void RevealArea(Board board, int area)
    {
        if (revealedAreas == null)
            revealedAreas = new List<int>();
        revealedAreas.Add(area);

        foreach (var thing in GetComponentsInChildren<Area>(true))
            if (thing.Id == area)
            {
                thing.gameObject.SetActive(true);
                TryRegisterSpawn(board, thing);
            }
    }

    void TryRegisterSpawn(Board board, Component thing)
    {
        if (thing.TryGetComponent<SpawnLocation>(out var spawn)
                && spawn.InstantCommanderSpawn
                && thing.TryGetComponent<Owner>(out var owner)
                && Players[owner.Id].State == PlayerInfo.PlayerState.Hidden)
            board.Register(Players[owner.Id]);
    }

    IEnumerable<T> Get<T>(int x, int y, int zMin, int zMax) => GetComponentsInChildren<GridPosition>()
        .Where(gp => gp.Position.x == x && gp.Position.y == y && gp.Position.z >= zMin && gp.Position.z <= zMax && gp.GetComponent<T>() != null)
        .Select(gp => gp.GetComponent<T>());

    public void Load(Board board)
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
                if (Mathf.Abs(thing.Height - neighbour.GetComponent<GridPosition>().Height) >= 2) continue;

                if (neighbour.Neighbours == null)
                    neighbour.Neighbours = new();
                if (tile.Neighbours == null)
                    tile.Neighbours = new();

                neighbour.Neighbours.Add(tile);
                tile.Neighbours.Add(neighbour);
            }

            if (tile.TryGetComponent<UnitSpawn>(out var spawn))
            {
                var instance = Instantiate(spawn.Prefab);
                if (thing.TryGetComponent<Owner>(out var owner) && owner.Id < Players.Length)
                    instance.Player = Players[owner.Id];
                tile.Place(instance, spawn.Facing);
            }
        }
        foreach (var tile in Tiles)
            if (tile.GetComponent<Area>() != null)
                tile.gameObject.SetActive(false);
            else
                TryRegisterSpawn(board, tile);
    }

    internal Tile TileOf(Unit unit)
    {
        return Tiles.FirstOrDefault(x => x.Unit == unit);
    }

    public void ShowTileIndicators(IEnumerable<ITilePattern> patterns)
    {
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
                    tile.CurrentIndicator = pattern.Selectable ? TargetIndicator.Valid : TargetIndicator.Invalid;
                    tile.SetIndicatorVisible(true, pattern.Tiles.Contains(Tile.Hovered));
                }
            }
        }
    }

    public Tile[] GetSpawnTiles(SelectionFilter filter, PlayerInfo player)
    {
        var id = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i] == player)
            {
                id = i;
                break;
            }
        }

        List<Tile> valid = new List<Tile>();
        foreach (var owner in GetComponentsInChildren<Owner>())
        {
            if (owner.Id != id || !owner.TryGetComponent<SpawnLocation>(out var _)) continue;
            var tile = owner.GetComponent<Tile>();
            if (TileMatches(tile, filter, player))
                valid.Add(tile);
        }
        return valid.ToArray();
    }

    public void HideTileSelectionPreview()
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            tile.CurrentIndicator = TargetIndicator.Hidden;
            tile.ClearPathingInfo();
            tile.SetIndicatorVisible(false, false);
            tile.PrimaryPattern = null;
        }
    }

    public Tile[] GetPlayerTiles(PlayerInfo player)
    {
        List<Tile> valid = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null || tile.Unit == null || tile.Unit.Player != player) continue;
            valid.Add(tile);
        }
        return valid.ToArray();
    }

    public bool TileMatches(Tile tile, SelectionFilter filter, PlayerInfo player)
    {
        if (!tile.gameObject.activeSelf)
            return false;
        var validTarget = ((filter.Tile & TileFilter.Empty) > 0) && (tile.Unit == null);
        if (tile.Unit != null || tile.HasObstacle)
        {
            var isObstacle = tile.HasObstacle || tile.Unit.Player == null;
            validTarget |= ((filter.Tile & TileFilter.Obstacle) > 0) && isObstacle;
            validTarget |= ((filter.Tile & TileFilter.Self) > 0) && !isObstacle && tile.Unit.Player == player;
            validTarget |= ((filter.Tile & TileFilter.Ally) > 0) && !isObstacle && tile.Unit.Player != player && tile.Unit.Player.Team == player.Team;
            validTarget |= ((filter.Tile & TileFilter.Enemy) > 0) && !isObstacle && (tile.Unit.Player.Team != player.Team);

            validTarget &= (filter.Unit & tile.Unit.Type) > 0;
        }

        // tile.SetIndicator(validTarget ? TargetIndicator.Valid : TargetIndicator.Invalid);
        return validTarget;
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