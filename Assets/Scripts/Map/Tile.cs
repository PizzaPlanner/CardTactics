using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public enum TargetIndicator
{
    Valid,
    Invalid,
    Hidden,
    Origin,
    Special
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

    [ShowInInspector] public PathingData Pathing;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject Marker;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject PathingIndicator, PIBorderLeft, PIBorderRight, PIBorderTop, PIBorderBottom;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject MovementIndicators, MICenter, MILeft, MIRight, MITop, MIBottom;

    [SerializeField]
    [ChildGameObjectsOnly]
    GameObject SelectionIndicator;

    void Awake()
    {
        Position = GetComponent<GridPosition>().Position;
        var unit = GetComponentInChildren<Unit>();
        if (unit != null)
            Place(unit, Facing.Preserve);
    }

    void Start()
    {
        CurrentIndicator = TargetIndicator.Hidden;
    }

    void OnMouseOver()
    {
        if (Hovered != null || EventSystem.current.IsPointerOverGameObject())
            return;
        Hovered = this;
        if (Unit != null)
            FindObjectOfType<UIDocument>().rootVisualElement.Query<PlayerInfoBadge>()
            .Where(x => x.UnitItem.Unit == Unit)
            .ForEach(x => x.AddToClassList("unit-badge--hovered"));
        SelectionIndicator.SetActive(true);
        if (PrimaryPattern != null)
        {
            var isPath = (PrimaryPattern is PathPattern);
            var tiles = PrimaryPattern.Tiles.ToArray();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].PaintIndicator(true);
                if (isPath)
                    tiles[i].ShowPath(i > 0 ? tiles[i - 1] : null, i < tiles.Length - 1 ? tiles[i + 1] : null);
            }
        }
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        if (Hovered != this)
            return;
        Hovered = null;
        if (Unit != null)
            FindObjectOfType<UIDocument>().rootVisualElement.Query<PlayerInfoBadge>()
            .Where(x => x.UnitItem.Unit == Unit)
            .ForEach(x => x.RemoveFromClassList("unit-badge--hovered"));
        SelectionIndicator.SetActive(false);
        if (PrimaryPattern != null)
        {
            var isPath = (PrimaryPattern is PathPattern);
            var tiles = PrimaryPattern.Tiles.ToArray();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].PaintIndicator(false);
                if (isPath)
                    tiles[i].HidePath();
            }
        }
    }

    void ShowPath(Tile from, Tile to)
    {
        MovementIndicators.SetActive(true);
        if (to == null)
            MICenter.SetActive(true);
        if (from != null)
        {
            if (from.Position.x > Position.x)
            {
                from.MILeft.SetActive(true);
                MIRight.SetActive(true);
            }
            else if (from.Position.x < Position.x)
            {
                from.MIRight.SetActive(true);
                MILeft.SetActive(true);
            }
            else if (from.Position.y > Position.y)
            {
                from.MIBottom.SetActive(true);
                MITop.SetActive(true);
            }
            else if (from.Position.y < Position.y)
            {
                from.MITop.SetActive(true);
                MIBottom.SetActive(true);
            }
        }
    }

    void HidePath()
    {
        MovementIndicators.SetActive(false);
        foreach (Transform t in MovementIndicators.transform)
            t.gameObject.SetActive(false);
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

    [ShowInInspector]
    public TargetIndicator CurrentIndicator { get; set; }

    public void SetIndicatorVisible(bool visible, bool highlightPattern)
    {
        PathingIndicator.gameObject.SetActive(visible);
        if (visible)
        {
            PIBorderLeft.gameObject.SetActive(true);
            PIBorderRight.gameObject.SetActive(true);
            PIBorderTop.gameObject.SetActive(true);
            PIBorderBottom.gameObject.SetActive(true);
            foreach (var nb in Neighbours)
            {
                if (nb.CurrentIndicator != CurrentIndicator || nb.CurrentIndicator == TargetIndicator.Hidden) continue;
                if (nb.Position.x > Position.x)
                {
                    PIBorderRight.gameObject.SetActive(false);
                    nb.PIBorderLeft.gameObject.SetActive(false);
                }
                else if (nb.Position.x < Position.x)
                {
                    PIBorderLeft.gameObject.SetActive(false);
                    nb.PIBorderRight.gameObject.SetActive(false);
                }
                if (nb.Position.y < Position.y)
                {
                    PIBorderBottom.gameObject.SetActive(false);
                    nb.PIBorderTop.gameObject.SetActive(false);
                }
                else if (nb.Position.y > Position.y)
                {
                    PIBorderTop.gameObject.SetActive(false);
                    nb.PIBorderBottom.gameObject.SetActive(false);
                }
            }
            PaintIndicator(highlightPattern);
        }
    }

    void PaintIndicator(bool highlightPattern)
    {
        var color = Color.black;
        switch (CurrentIndicator)
        {
            case TargetIndicator.Invalid: color = new Color(1, 0, 0, 0.7f); break;
            case TargetIndicator.Valid: color = new Color(0, 0, 1, 0.7f); break;
            case TargetIndicator.Origin: color = new Color(0.3f, .7f, .5f, 0.7f); break;
            case TargetIndicator.Hidden: color = new Color(0, 0, 0, 0); break;
            case TargetIndicator.Special: color = new Color(0, 1, 0, 0); break;
        }
        if (highlightPattern)
            color += new Color(0.2f, 0.2f, 0.2f);
        PathingIndicator.GetComponent<Renderer>().material.color = color;
        foreach (var border in PathingIndicator.GetComponentsInChildren<Renderer>(true))
            border.material.color = color * 0.8f;
    }

    public bool CanPlace(Unit unit)
    {
        return Unit == null;
    }

    public bool Place(Unit unit, Facing facing)
    {
        if (!CanPlace(unit))
            return false;

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

    public Vector3 UnitAnchor => transform.position + new Vector3(0, 0.5f, 0);

    public bool HasObstacle { get; internal set; }

    void PlaceUnitNoCheck(Unit unit, Facing facing)
    {
        unit.transform.position = UnitAnchor;
        unit.transform.SetParent(transform);
        unit.transform.rotation = Quaternion.AngleAxis(FaceToRot(facing), Vector3.up);
        unit.gameObject.SetActive(true);
    }
}