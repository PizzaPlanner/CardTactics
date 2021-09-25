using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Map : ScriptableObject
{
    [System.Serializable]
    internal class TilePrefabWithInfo
    {
        public Tile Prefab;
        public int Area;
        public int Trigger;
        public int Height;
        public int MarkerId;
        public int Owner;
        public Unit UnitPrefab;
    }

    [System.Serializable]
    internal class MapInfo
    {
        internal Vector2Int Size
        {
            get => size; set
            {
                var data = new List<TilePrefabWithInfo>();
                for (int yOld = 0; yOld < size.y; yOld++)
                {
                    for (int xOld = 0; xOld < size.x; xOld++)
                    {
                        data.Add(this[xOld, yOld]);
                    }
                    for (int xNew = size.x; xNew < value.x; xNew++)
                    {
                        data.Add(new TilePrefabWithInfo());
                    }
                }
                for (int yNew = size.y; yNew < value.y; yNew++)
                {
                    for (int xNew = 0; xNew < value.x; xNew++)
                    {
                        data.Add(new TilePrefabWithInfo());
                    }
                }
                size = value;
                Data = data;
            }
        }
        [SerializeField] Vector2Int size;
        [SerializeReference] internal List<TilePrefabWithInfo> Data;

        internal TilePrefabWithInfo this[int x, int y]
        {
            get => Data[x + Size.x * y];
            set { Data[x + Size.x * y] = value; }
        }
    }

    [SerializeReference] MapInfo TilePrefabs;

    [SerializeReference] Trigger[] Triggers;

    [SerializeField] public PlayerInfo[] Players;

    Tile[,] Tiles;

    List<int> ActivatedTriggers;

    public List<(Tile tile, Trigger trigger)> CheckTriggers(List<PlayerInfo> died)
    {
        var l = new List<(Tile tile, Trigger trigger)>();
        foreach (var player in died)
        {
            if (player.OnLooseTrigger > 0 && player.OnLooseTrigger <= Triggers.Length && !ActivatedTriggers.Contains(player.OnLooseTrigger))
            {
                ActivatedTriggers.Add(player.OnLooseTrigger);
                l.Add((null, Triggers[player.OnLooseTrigger - 1]));
            }
        }

        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            var trigger = TilePrefabs[tile.Position.x, tile.Position.y].Trigger;
            if (trigger > 0 && Triggers.Length > trigger - 1
                && !ActivatedTriggers.Contains(trigger)
                && Triggers[trigger - 1].ShouldTrigger(this, tile))
            {
                ActivatedTriggers.Add(trigger);
                l.Add((tile, Triggers[trigger - 1]));
            }
        }
        return l;
    }

    public void ShowMarker(int id)
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            if (TilePrefabs[tile.Position.x, tile.Position.y].MarkerId == id)
                tile.SetMarker(true);
        }
    }

    public void HideMarker(int id)
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            if (TilePrefabs[tile.Position.x, tile.Position.y].MarkerId == id)
                tile.SetMarker(false);
        }
    }

    public void RevealArea(int area)
    {
        foreach (var tile in Tiles)
        {
            if (tile != null && tile.Area == area)
            {
                tile.gameObject.SetActive(true);
                if (TilePrefabs[tile.Position.x, tile.Position.y].UnitPrefab != null)
                    tile.Place(Instantiate(TilePrefabs[tile.Position.x, tile.Position.y].UnitPrefab), Facing.Preserve);
            }
        }
    }

    public static Map Create(Map prefab)
    {
        var map = Instantiate(prefab);
        map.Players = new PlayerInfo[prefab.Players.Length];
        for (int i = 0; i < prefab.Players.Length; i++)
        {
            map.Players[i] = new PlayerInfo
            {
                Deck = Deck.Create(prefab.Players[i].Deck),
                Color = prefab.Players[i].Color,
                Team = prefab.Players[i].Team,
                IsAI = prefab.Players[i].IsAI,
                RegisterLater = prefab.Players[i].RegisterLater,
                LooseCondition = prefab.Players[i].LooseCondition,
                OnLooseTrigger = prefab.Players[i].OnLooseTrigger
            };
        }

        map.ActivatedTriggers = new List<int>();
        map.Tiles = new Tile[map.TilePrefabs.Size.x, map.TilePrefabs.Size.y];
        for (int w = 0; w < map.TilePrefabs.Size.x; w++)
        {
            for (int h = 0; h < map.TilePrefabs.Size.y; h++)
            {
                if (map.TilePrefabs[w, h].Prefab == null) continue;
                var data = map.TilePrefabs[w, h];
                var tile = Instantiate(data.Prefab, new Vector3(w - map.TilePrefabs.Size.x / 2, data.Height * 0.25f, h - map.TilePrefabs.Size.y / 2), Quaternion.identity).GetComponent<Tile>();
                tile.Area = data.Area;
                map.Tiles[w, h] = tile;
                tile.Position = (w, h);
                tile.Neighbours = new List<Tile>();
                if (w > 0 && map.Tiles[w - 1, h] != null
                    && Mathf.Abs(map.TilePrefabs[w, h].Height - map.TilePrefabs[w - 1, h].Height) < 2)
                {
                    map.Tiles[w, h].Neighbours.Add(map.Tiles[w - 1, h]);
                    map.Tiles[w - 1, h].Neighbours.Add(map.Tiles[w, h]);
                }
                if (h > 0 && map.Tiles[w, h - 1] != null
                    && Mathf.Abs(map.TilePrefabs[w, h].Height - map.TilePrefabs[w, h - 1].Height) < 2)
                {
                    map.Tiles[w, h].Neighbours.Add(map.Tiles[w, h - 1]);
                    map.Tiles[w, h - 1].Neighbours.Add(map.Tiles[w, h]);
                }
                tile.gameObject.SetActive(false);
            }
        }

        return map;
    }

    public void ShowTileIndicators()
    {
        foreach (var tile in Tiles)
            tile?.SetIndicatorVisible(true);
    }

    public Tile[] ShowSpawnTiles(TargetType filter, PlayerInfo player)
    {
        var id = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i] == player)
            {
                id = i + 1;
                break;
            }
        }

        List<Tile> valid = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null || TilePrefabs[tile.Position.x, tile.Position.y].Owner != id || TilePrefabs[tile.Position.x, tile.Position.y].UnitPrefab != null) continue;
            if (TileMatches(tile, filter, player))
                valid.Add(tile);
        }
        return valid.ToArray();
    }

    public Tile[] ShowPlacementTiles(TargetType filter, PlayerInfo player)
    {
        List<Tile> valid = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
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
            tile.SetIndicator(TargetIndicator.Hidden);
            tile.ClearPathingInfo();
            tile.SetIndicatorVisible(false);
        }
    }

    public bool TileMatches(Tile tile, TargetType filter, PlayerInfo player)
    {
        if (!tile.gameObject.activeSelf)
            return false;
        var validTarget = ((filter & TargetType.Empty) > 0) && (tile.Unit == null);
        if (tile.Unit != null)
        {
            var isObstacle = tile.Unit.Player == null;
            validTarget |= ((filter & TargetType.Obstacle) > 0) && isObstacle;
            validTarget |= ((filter & TargetType.Ally) > 0) && !isObstacle && (tile.Unit.Player.Team == player.Team);
            validTarget |= ((filter & TargetType.Enemy) > 0) && !isObstacle && (tile.Unit.Player?.Team != player.Team);
        }

        tile.SetIndicator(validTarget ? TargetIndicator.Valid : TargetIndicator.Invalid);
        return validTarget;
    }

    public Tile[] ShowRange(Tile origin, int minRange, int maxRange, TargetType filter, RangeType type, PlayerInfo player)
    {
        List<Tile> valid = new List<Tile>();
        List<Tile> rangeTiles = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null || !tile.gameObject.activeSelf) continue;

            // Check range
            if (Tile.Distance(origin, tile) > maxRange || Tile.Distance(origin, tile) < minRange)
            {
                tile.SetIndicator(TargetIndicator.Hidden);
                continue;
            }

            // Check TargetType
            if (TileMatches(tile, filter, player))
            {
                valid.Add(tile);
                rangeTiles.Add(tile);
            }
        }
        if (type == RangeType.Pathfinding)
        {
            foreach (var tile in rangeTiles)
            {
                if (!Tile.FindPath(origin, tile, maxRange, rangeTiles))
                {
                    tile.SetIndicator(TargetIndicator.Hidden);
                    valid.Remove(tile);
                }
            }
        }
        return valid.ToArray();
    }
}