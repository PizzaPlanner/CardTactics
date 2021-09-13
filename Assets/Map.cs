using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class Map : SerializedScriptableObject
{
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    Tile[,] Tiles;

#if UNITY_EDITOR
    enum MapEditMode
    {
        Tile,
        Area,
        Trigger
    }

    [EnumToggleButtons]
    [ShowInInspector]
    static MapEditMode editMode;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Tile)]
    [AssetList]
    static Tile drawSelected;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Area)]
    [Range(0, 25)]
    static int drawArea;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Trigger)]
    [Range(1, 25)]
    static int drawTrigger;

    Trigger DrawTriggers(Trigger value)
    {
        var x = -1;
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < Triggers.Length; i++)
        {
            if (Triggers[i] == value)
            {
                if (x != -1)
                {
                    Debug.LogError("The same trigger is referenced multiple Times!");
                }
                else
                {
                    x = i;
                }
            }

        }
        EditorGUILayout.LabelField((x + 1).ToString(), GUILayout.Width(25));

        var trigger = (Trigger)EditorGUILayout.ObjectField(value, typeof(Trigger), false);
        EditorGUILayout.EndHorizontal();
        return trigger;
    }

    static TilePrefabWithInfo DrawCell(Rect rect, TilePrefabWithInfo value)
    {
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && rect.Contains(Event.current.mousePosition))
        {
            if (value == null)
                value = new TilePrefabWithInfo();
            if (editMode == MapEditMode.Tile)
            {
                if (Event.current.button == 0)
                    value.Prefab = drawSelected;
                else
                    value.Prefab = null;
            }
            else if (editMode == MapEditMode.Area)
            {
                if (Event.current.button == 0)
                    value.Area = drawArea;
                else
                    value.Area = 0;
            }
            else if (editMode == MapEditMode.Trigger)
            {
                if (Event.current.button == 0)
                    value.Trigger = drawTrigger;
                else
                    value.Trigger = 0;
            }
            GUI.changed = true;
            Event.current.Use();
        }

        if (value?.Prefab != null)
        {
            GUI.DrawTexture(rect, value.Prefab.EditorImage);
            if (editMode == MapEditMode.Area)
                GUI.Label(rect, value.Area.ToString());
            if (editMode == MapEditMode.Trigger && value.Trigger > 0)
                GUI.Label(rect, value.Trigger.ToString());
        }
        return value;
    }
#endif

    [System.Serializable]
    class TilePrefabWithInfo
    {
        public Tile Prefab;
        public int Area;
        public int Trigger;
    }

    [TitleGroup("Map")]
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawCell", Transpose = true)]
    [SerializeField]
    readonly TilePrefabWithInfo[,] TilePrefabs;

    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]

    [CustomValueDrawer("DrawTriggers")]
    [SerializeField] Trigger[] Triggers;

    List<int> ActivatedTriggers;

    public void CheckTriggers(GameSettings settings)
    {
        Debug.Log("Check trigger matches");
        foreach (var tile in Tiles)
        {
            if (tile == null) continue;
            var trigger = TilePrefabs[tile.Position.x, tile.Position.y].Trigger;
            if (trigger > 0 && Triggers.Length > trigger - 1 && Triggers[trigger - 1].ShouldTrigger(tile))
                Trigger(settings, trigger);
        }
    }

    public void Trigger(GameSettings settings, int id)
    {
        if (ActivatedTriggers.Contains(id))
            return;
        Debug.Log("Triggering " + id);
        ActivatedTriggers.Add(id);
        if (Triggers.Length <= id - 1)
        {
            Debug.LogError("Trigger " + id + " is not defined!");
            return;
        }

        Triggers[id - 1].Execute(settings);
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
                var tile = Instantiate(TilePrefabs[w, h].Prefab, new Vector3(w - Width / 2, 0, h - Height / 2), Quaternion.identity).GetComponent<Tile>();
                tile.Area = TilePrefabs[w, h].Area;
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
                tile.gameObject.SetActive(false);
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