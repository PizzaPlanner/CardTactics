using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class Map : SerializedScriptableObject
{
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawCell")]
    [SerializeField]
    Tile[,] TilePrefabs;

    Tile[,] Tiles;

#if UNITY_EDITOR
    [AssetList] [SerializeField] Tile drawSelected;
    Tile DrawCell(Rect rect, Tile value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0)
                value = drawSelected;
            else
                value = null;
            GUI.changed = true;
            Event.current.Use();
        }

        if (value != null)
            GUI.DrawTexture(rect, value.EditorImage);
        return value;
    }
#endif

    public void Create()
    {
        Tiles = new Tile[TilePrefabs.GetLength(0), TilePrefabs.GetLength(1)];
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                if (TilePrefabs[w, h] == null) continue;
                var tile = Instantiate(TilePrefabs[w, h], new Vector3(w - Width / 2, 0, h - Height / 2), Quaternion.identity).GetComponent<Tile>();
                Tiles[w, h] = tile;
                tile.Position = (w, h);
                tile.Neighbours = new List<Tile>();
                if (w > 0 && Tiles[w - 1, h] != null)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w - 1, h]);
                    Tiles[w - 1, h].Neighbours.Add(Tiles[w, h]);
                }
                if (h > 0 && Tiles[w, h - 1] != null)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w, h - 1]);
                    Tiles[w, h - 1].Neighbours.Add(Tiles[w, h]);
                }
            }
        }
    }

    public int ShowPlacementTiles(TargetType filter, GameSettings.PlayerInfo player)
    {
        int count = 0;
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            count += TileMatches(tile, filter, player) ? 1 : 0;
        }
        return count;
    }

    public void HideTileSelectionPreview()
    {
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            tile.SetIndicator(TargetIndicator.Hidden);
            tile.ClearPathingInfo();
        }
    }

    public bool TileMatches(Tile tile, TargetType filter, GameSettings.PlayerInfo player)
    {
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

    public int ShowRange(Tile origin, int minRange, int maxRange, TargetType filter, RangeType type, GameSettings.PlayerInfo player)
    {
        int matches = 0;
        List<Tile> rangeTiles = new List<Tile>();
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;

            // Check range
            if (Tile.Distance(origin, tile) > maxRange || Tile.Distance(origin, tile) < minRange)
            {
                tile.SetIndicator(TargetIndicator.Hidden);
                continue;
            }

            // Check TargetType
            if (TileMatches(tile, filter, player))
            {
                matches++;
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
                    matches--;
                }
            }
        }
        return matches;
    }
}