using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public enum TargetIndicator
{
    Valid,
    Invalid,
    Hidden,
    Preview
}

public enum Facing
{
    Left,
    Right,
    Up,
    Down,
    Preserve
}

[RequireComponent(typeof(GridPosition))]
public class Tile : MonoBehaviour
{
    public struct PathingData
    {
        public int G, H;
        public int F => G + H;
        public List<Tile> LastCalculatedPath;
    }

    internal static Facing GetFacing(Tile from, Tile to)
    {
        var absx = Mathf.Abs(from.Position.x - to.Position.x);
        var absy = Mathf.Abs(from.Position.y - to.Position.y);
        if (absx > absy)
        {
            if (from.Position.x < to.Position.x)
                return Facing.Right;
            else if (from.Position.x > to.Position.x)
                return Facing.Left;
        }
        else
        {
            if (from.Position.y < to.Position.y)
                return Facing.Up;
            else if (from.Position.y > to.Position.y)
                return Facing.Down;
        }
        return Facing.Preserve;
    }

    public Unit Unit;

    public ITilePattern PrimaryPattern { get; set; }

    public Vector3Int Position { get; private set; }

    public static Tile Hovered { get; private set; }
    public List<Tile> Neighbours;

    public PathingData Pathing;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject Marker;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject SelectionIndicator;

    [SerializeField]
    [ChildGameObjectsOnly]
    MeshRenderer Visuals;

    void Awake()
    {
        Position = GetComponent<GridPosition>().Position;
    }

    void Start()
    {
        CurrentIndicator = TargetIndicator.Hidden;
    }

    public void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
            Hover();
    }

    public void Hover()
    {
        if (Hovered != null)
            return;
        Hovered = this;
        if (Unit != null)
            FindObjectOfType<TurnPanel>().OnHover(Unit, true);
        SelectionIndicator.SetActive(true);
        if (PrimaryPattern != null)
        {
            var isPath = PrimaryPattern is PathPattern;
            var tiles = PrimaryPattern.Tiles.ToArray();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].PaintIndicator(true, true);
                if (isPath)
                    tiles[i].ShowPath(i > 0 ? tiles[i - 1] : null, i < tiles.Length - 1 ? tiles[i + 1] : null);
            }
        }
    }

    public void OnMouseLeave()
    {
        if (Hovered != this)
            return;
        Hovered = null;
        if (Unit != null)
            FindObjectOfType<TurnPanel>().OnHover(Unit, false);
        SelectionIndicator.SetActive(false);
        if (PrimaryPattern != null)
        {
            var isPath = (PrimaryPattern is PathPattern);
            var tiles = PrimaryPattern.Tiles.ToArray();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].PaintIndicator(true, false);
                if (isPath)
                    tiles[i].HidePath();
            }
        }
    }

    void Update()
    {
        if (Hovered != this) return;
        if (Input.GetMouseButtonDown(0) && CurrentIndicator == TargetIndicator.Valid && PrimaryPattern != null)
        {
            NetworkClient.Send(new SelectTileMessage { Position = Position });
            Debug.Log("Sending selection message");
        }
    }

    void ShowPath(Tile from, Tile to)
    {
        // if (to == null)
        //     MICenter.SetActive(true);
        // if (from != null)
        // {
        //     if (from.Position.x > Position.x)
        //     {
        //         from.MILeft.SetActive(true);
        //         MIRight.SetActive(true);
        //     }
        //     else if (from.Position.x < Position.x)
        //     {
        //         from.MIRight.SetActive(true);
        //         MILeft.SetActive(true);
        //     }
        //     else if (from.Position.y > Position.y)
        //     {
        //         from.MIBottom.SetActive(true);
        //         MITop.SetActive(true);
        //     }
        //     else if (from.Position.y < Position.y)
        //     {
        //         from.MITop.SetActive(true);
        //         MIBottom.SetActive(true);
        //     }
        // }
    }

    void HidePath()
    {
        // MovementIndicators.SetActive(false);
        // foreach (Transform t in MovementIndicators.transform)
        //     t.gameObject.SetActive(false);
    }

    public void SetMarker(bool active)
    {
        Marker.gameObject.SetActive(active);
    }

    public static int Distance(Tile start, Tile end) => Mathf.Abs(start.Position.x - end.Position.x) + Mathf.Abs(start.Position.y - end.Position.y);

    public static bool FindPath(Tile start, Tile end, int maxLength, List<Tile> valid)
    {
        if (end.Pathing.LastCalculatedPath != null)
            return true;

        foreach (var tile in valid)
        {
            tile.Pathing.H = Distance(tile, end);
            tile.Pathing.G = short.MaxValue;
        }

        start.Pathing.G = 0;
        start.Pathing.LastCalculatedPath = new List<Tile> { start };

        var toTest = new List<Tile> { start };

        while (toTest.Count > 0)
        {
            toTest.Sort((x, y) => x.Pathing.F.CompareTo(y.Pathing.F));
            var current = toTest[0];
            if (current == end)
                return true;

            toTest.RemoveAt(0);

            foreach (var neighbour in current.Neighbours)
            {
                if (!valid.Contains(neighbour))
                    continue;
                int smallG = current.Pathing.G + 1;
                if (smallG < neighbour.Pathing.G && smallG <= maxLength)
                {
                    neighbour.Pathing.G = smallG;
                    neighbour.Pathing.H = Distance(neighbour, end);
                    if (!toTest.Contains(neighbour))
                    {
                        toTest.Add(neighbour);
                        neighbour.Pathing.LastCalculatedPath = new List<Tile>();
                        if (current.Pathing.LastCalculatedPath != null)
                            neighbour.Pathing.LastCalculatedPath.AddRange(current.Pathing.LastCalculatedPath);
                        neighbour.Pathing.LastCalculatedPath.Add(neighbour);
                    }
                }
            }
        }
        return false;
    }

    public Facing GetPathFacing()
    {
        if (Pathing.LastCalculatedPath?.Count >= 2)
        {
            var c = Pathing.LastCalculatedPath.Count;
            return GetFacing(Pathing.LastCalculatedPath[c - 2], Pathing.LastCalculatedPath[c - 1]);
        }
        return Facing.Preserve;
    }

    public TargetIndicator CurrentIndicator { get; set; }

    Vector4 NeighbourState;
    public void SetIndicatorVisible(bool visible, bool highlightPattern)
    {
        var myst = visible ? Vector4.one : Vector4.zero;
        if (visible)
            foreach (var nb in Neighbours)
            {
                if (nb.CurrentIndicator != CurrentIndicator || nb.CurrentIndicator == TargetIndicator.Hidden) continue;
                var nbst = nb.NeighbourState;
                if (nb.Position.x > Position.x)
                {
                    myst.x = 0;
                    nbst.y = 0;
                }
                else if (nb.Position.x < Position.x)
                {
                    myst.y = 0;
                    nbst.x = 0;
                }
                if (nb.Position.y < Position.y)
                {
                    myst.w = 0;
                    nbst.z = 0;
                }
                else if (nb.Position.y > Position.y)
                {
                    myst.z = 0;
                    nbst.w = 0;
                }
                nb.NeighbourState = nbst;
                nb.Visuals.material.SetVector("_edgeState", nb.NeighbourState);
            }
        NeighbourState = myst;
        Visuals.material.SetVector("_edgeState", NeighbourState);
        PaintIndicator(visible, highlightPattern);
    }
    void PaintIndicator(bool visible, bool highlightPattern)
    {
        var material = Visuals.material;
        // Plane
        var dict = highlightPattern ? GameSettings.Instance.TileIndicatorHighlightColors : GameSettings.Instance.TileIndicatorColors;
        if (dict.ContainsKey(CurrentIndicator) && visible)
            material.SetColor("_centerColor", dict[CurrentIndicator]);
        else
            material.SetColor("_centerColor", Color.clear);

        // Borders
        dict = highlightPattern ? GameSettings.Instance.BorderIndicatorHighlightColors : GameSettings.Instance.BorderIndicatorColors;
        if (visible)
        {
            if (dict.ContainsKey(CurrentIndicator))
                material.SetColor("_highlightColor", dict[CurrentIndicator]);
            else
                material.SetColor("_highlightColor", Color.clear);
        }
    }

    public bool Place(Unit unit, Facing facing, bool updatePosition)
    {
        if ((Unit != null && Unit != unit && !Unit.IsDead) || unit == null)
            return false;

        if (updatePosition)
            PlaceUnitNoCheck(unit, facing);
        Unit = unit;
        return true;
    }

    internal void ClearPathingInfo()
    {
        Pathing = default;
        Pathing.LastCalculatedPath = null;
        HidePath();
    }

    public int FaceToRot(Facing face)
    {
        if (face == Facing.Left)
            return 270;
        else if (face == Facing.Right)
            return 90;
        else if (face == Facing.Up)
            return 0;
        else if (face == Facing.Down)
            return 180;
        return (int)(Unit != null ? Unit.transform.rotation.eulerAngles.y : 0);
    }

    public Facing RotToFace(float rot)
    {
        rot %= 360;
        if (rot >= 225 && rot < 315)
            return Facing.Left;
        else if (rot >= 45 && rot < 135)
            return Facing.Right;
        else if (rot >= 315 || rot < 45)
            return Facing.Up;
        else if (rot >= 135 && rot < 225)
            return Facing.Down;
        return Facing.Preserve;
    }

    public void Face(Facing targetFacing)
    {
        if (Unit == null) return;
        PlaceUnitNoCheck(Unit, targetFacing);
    }

    public Facing Facing => RotToFace(Unit.transform.rotation.eulerAngles.y);

    public Vector3 UnitAnchor => transform.position;

    void PlaceUnitNoCheck(Unit unit, Facing facing)
    {
        unit.transform.position = UnitAnchor;
        unit.transform.SetParent(transform);
        unit.transform.rotation = Quaternion.AngleAxis(FaceToRot(facing), Vector3.up);
    }
}