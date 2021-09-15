using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public partial class Map : SerializedScriptableObject
{
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    Tile[,] Tiles;

    [System.Serializable]
    class TilePrefabWithInfo
    {
        public Tile Prefab;
        public int Area;
        public int Trigger;
        public int Height;
        public int MarkerId;
        public int SpawnId;
    }

    [TitleGroup("Map")]
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawCell", Transpose = true)]
    [SerializeField]
    readonly TilePrefabWithInfo[,] TilePrefabs;

    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]

    [CustomValueDrawer("DrawTriggers")]
    [SerializeField] Trigger[] Triggers;

    List<int> ActivatedTriggers;

    public IEnumerator CheckTriggers(GameSettings settings, Action onFinish)
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            var trigger = TilePrefabs[tile.Position.x, tile.Position.y].Trigger;
            if (trigger > 0 && Triggers.Length > trigger - 1 && Triggers[trigger - 1].ShouldTrigger(tile))
                yield return Trigger(settings, trigger, tile);
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
        Tiles = new Tile[TilePrefabs.GetLength(0), TilePrefabs.GetLength(1)];
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                if (TilePrefabs[w, h]?.Prefab == null) continue;
                var data = TilePrefabs[w, h];
                var tile = Instantiate(TilePrefabs[w, h].Prefab, new Vector3(w - Width / 2, data.Height * 0.25f, h - Height / 2), Quaternion.identity).GetComponent<Tile>();
                tile.Area = data.Area;
                Tiles[w, h] = tile;
                tile.Position = (w, h);
                tile.Neighbours = new List<Tile>();
                if (w > 0 && Tiles[w - 1, h] != null && Mathf.Abs(TilePrefabs[w, h].Height - TilePrefabs[w - 1, h].Height) < 2)
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

    public Tile[] ShowSpawnTiles(TargetType filter, GameSettings settings, GameSettings.PlayerInfo player)
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

    public Tile[] ShowPlacementTiles(TargetType filter, GameSettings.PlayerInfo player)
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

    public bool TileMatches(Tile tile, TargetType filter, GameSettings.PlayerInfo player)
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

    public Tile[] ShowRange(Tile origin, int minRange, int maxRange, TargetType filter, RangeType type, GameSettings.PlayerInfo player)
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