using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileType
{
    Grass,
    Mud,
    Snow,
    Wall
}

public enum TargetIndicator
{
    Valid,
    Invalid,
    Hidden,
    Path,
    Origin
}

public enum Facing
{
    Left,
    Right,
    Up,
    Down,
    Preserve
}

public class Tile : MonoBehaviour
{

#if UNITY_EDITOR
    public Texture2D EditorImage;
#endif
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
                return Facing.Left;
            else if (from.Position.x > to.Position.x)
                return Facing.Right;
        }
        else
        {
            if (from.Position.y < to.Position.y)
                return Facing.Down;
            else if (from.Position.y > to.Position.y)
                return Facing.Up;
        }
        return Facing.Preserve;
    }

    public int Area;

    public TileType Type;

    public Unit Unit;

    public (int x, int y) Position;

    public static Tile Hovered { get; private set; }
    public List<Tile> Neighbours;

    public PathingData Pathing;

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

    void Start()
    {
        CurrentIndicator = TargetIndicator.Hidden;
    }

    void OnMouseOver()
    {
        if (Hovered != null || EventSystem.current.IsPointerOverGameObject())
            return;
        Hovered = this;
        SelectionIndicator.SetActive(true);
        if (Pathing.LastCalculatedPath != null)
        {
            foreach (var tile in Pathing.LastCalculatedPath)
                tile.ShowPath();
            MICenter.SetActive(true);
        }
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        if (Hovered != this)
            return;
        Hovered = null;
        SelectionIndicator.SetActive(false);
        if (Pathing.LastCalculatedPath != null)
        {
            foreach (var tile in Pathing.LastCalculatedPath)
                tile.HidePath();
        }
    }

    void ShowPath()
    {
        MovementIndicators.SetActive(true);
        if (Pathing.LastCalculatedPath.Count < 2) return;
        var prev = Pathing.LastCalculatedPath[^2];
        if (prev != null)
        {
            if (prev.Position.x > Position.x)
            {
                prev.MILeft.SetActive(true);
                MIRight.SetActive(true);
            }
            else if (prev.Position.x < Position.x)
            {
                prev.MIRight.SetActive(true);
                MILeft.SetActive(true);
            }
            else if (prev.Position.y > Position.y)
            {
                prev.MIBottom.SetActive(true);
                MITop.SetActive(true);
            }
            else if (prev.Position.y < Position.y)
            {
                prev.MITop.SetActive(true);
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

    public event Action<Tile> OnClickedValidTarget;

    void OnMouseDown()
    {
        if (CurrentIndicator == TargetIndicator.Path || CurrentIndicator == TargetIndicator.Valid)
        {
            OnClickedValidTarget?.Invoke(this);
        }
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

    public TargetIndicator CurrentIndicator { get; private set; }

    public void SetIndicatorVisible(bool visible)
    {
        PathingIndicator.gameObject.SetActive(visible);
        if (visible)
        {
            var left = true;
            var right = true;
            var top = true;
            var bottom = true;
            foreach (var nb in Neighbours)
            {
                if (nb.CurrentIndicator != CurrentIndicator || nb.CurrentIndicator == TargetIndicator.Hidden) continue;
                if (nb.Position.x > Position.x) right = false;
                else if (nb.Position.x < Position.x) left = false;
                if (nb.Position.y < Position.y) bottom = false;
                if (nb.Position.y > Position.y) top = false;
            }
            PIBorderLeft.gameObject.SetActive(left);
            PIBorderRight.gameObject.SetActive(right);
            PIBorderTop.gameObject.SetActive(top);
            PIBorderBottom.gameObject.SetActive(bottom);
        }
    }

    public void SetIndicator(TargetIndicator state)
    {
        var color = Color.black;
        switch (state)
        {
            case TargetIndicator.Invalid: color = new Color(1, 0, 0, 0.7f); break;
            case TargetIndicator.Path: color = new Color(0, 0.6f, 1, 0.7f); break;
            case TargetIndicator.Valid: color = new Color(0, 0, 1, 0.7f); break;
            case TargetIndicator.Origin: color = new Color(0.3f, .7f, .5f, 0.7f); break;
            case TargetIndicator.Hidden: color = new Color(0, 0, 0, 0); break;
        }
        PathingIndicator.GetComponent<Renderer>().material.color = color;
        foreach (var border in PathingIndicator.GetComponentsInChildren<Renderer>(true))
            border.material.color = color * 0.8f;
        CurrentIndicator = state;
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
            return 90;
        else if (face == Facing.Right)
            return 270;
        else if (face == Facing.Up)
            return 180;
        else if (face == Facing.Down)
            return 0;
        return (int)(Unit?.transform.rotation.eulerAngles.y ?? 0);
    }

    public Facing RotToFace(float rot)
    {
        rot %= 360;
        if (rot >= 225 && rot < 315)
            return Facing.Right;
        else if (rot >= 45 && rot < 135)
            return Facing.Left;
        else if (rot >= 315 || rot < 45)
            return Facing.Down;
        else if (rot >= 135 && rot < 225)
            return Facing.Up;
        return Facing.Preserve;
    }

    public void Face(Facing targetFacing)
    {
        if (Unit == null) return;
        PlaceUnitNoCheck(Unit, targetFacing);
    }

    public Facing Facing => RotToFace(Unit.transform.rotation.eulerAngles.y);

    void PlaceUnitNoCheck(Unit unit, Facing facing)
    {
        unit.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        unit.transform.SetParent(transform);
        unit.transform.rotation = Quaternion.AngleAxis(FaceToRot(facing), Vector3.up);
        unit.gameObject.SetActive(true);
    }
}