using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public partial class Map : ScriptableObject
{

    [System.Serializable]
    internal class TilePrefabWithInfo
    {
        public Tile Prefab;
        public int Area;
        public int Trigger;
        public int Height;
        public int MarkerId;
        public int SpawnId;
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

    Tile[,] Tiles;

    List<int> ActivatedTriggers;

    public IEnumerator CheckTriggers(GameSettings settings, Action onFinish)
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            var trigger = TilePrefabs[tile.Position.x, tile.Position.y].Trigger;
            if (trigger > 0 && Triggers.Length > trigger - 1 && Triggers[trigger - 1].ShouldTrigger(settings, tile))
                yield return Trigger(settings, trigger, tile);
        }
        foreach (var player in settings.Players)
        {
            if (player.OnLooseTrigger?.ShouldTrigger(settings, null) == true)
                yield return player.OnLooseTrigger.Execute(settings, null);
        }
        onFinish();
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

    public IEnumerator Trigger(GameSettings settings, int id, Tile tile)
    {
        if (ActivatedTriggers.Contains(id))
            yield break;
        ActivatedTriggers.Add(id);
        if (Triggers.Length <= id - 1)
        {
            Debug.LogError("Trigger " + id + " is not defined!");
            yield break;
        }

        yield return Triggers[id - 1].Execute(settings, tile);
    }

    public void RevealArea(int area)
    {
        foreach (var tile in Tiles)
        {
            if (tile != null && tile.Area == area)
                tile.gameObject.SetActive(true);
        }
    }

    public void Create()
    {
        ActivatedTriggers = new List<int>();
        Tiles = new Tile[TilePrefabs.Size.x, TilePrefabs.Size.y];
        for (int w = 0; w < TilePrefabs.Size.x; w++)
        {
            for (int h = 0; h < TilePrefabs.Size.y; h++)
            {
                if (TilePrefabs[w, h].Prefab == null) continue;
                var data = TilePrefabs[w, h];
                var tile = Instantiate(data.Prefab, new Vector3(w - TilePrefabs.Size.x / 2, data.Height * 0.25f, h - TilePrefabs.Size.y / 2), Quaternion.identity).GetComponent<Tile>();
                tile.Area = data.Area;
                Tiles[w, h] = tile;
                tile.Position = (w, h);
                tile.Neighbours = new List<Tile>();
                if (w > 0 && Tiles[w - 1, h] != null
                    && Mathf.Abs(TilePrefabs[w, h].Height - TilePrefabs[w - 1, h].Height) < 2)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w - 1, h]);
                    Tiles[w - 1, h].Neighbours.Add(Tiles[w, h]);
                }
                if (h > 0 && Tiles[w, h - 1] != null
                    && Mathf.Abs(TilePrefabs[w, h].Height - TilePrefabs[w, h - 1].Height) < 2)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w, h - 1]);
                    Tiles[w, h - 1].Neighbours.Add(Tiles[w, h]);
                }
                tile.gameObject.SetActive(false);
            }
        }
    }

    public void ShowTileIndicators()
    {
        foreach (var tile in Tiles)
            tile?.SetIndicatorVisible(true);
    }

    public Tile[] ShowSpawnTiles(TargetType filter, GameSettings settings, PlayerInfo player)
    {
        var id = 0;
        for (int i = 0; i < settings.Players.Length; i++)
        {
            if (settings.Players[i] == player)
            {
                id = i + 1;
                break;
            }
        }

        List<Tile> valid = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null || TilePrefabs[tile.Position.x, tile.Position.y].SpawnId != id) continue;
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