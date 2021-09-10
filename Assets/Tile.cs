using System;
using System.Collections.Generic;
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
    Path
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

    public TileType Type;

    public Unit Unit;

    Renderer render;

    public (int x, int y) Position;

    public static Tile Hovered { get; private set; }
    public List<Tile> Neighbours;

    public PathingData Pathing;

    void Start()
    {
        render = GetComponent<Renderer>();
        if (Unit != null)
        {
            Unit = Instantiate(Unit);
            PlaceUnitNoCheck(Unit, 0);
        }
        CurrentIndicator = TargetIndicator.Hidden;
    }
    void OnMouseEnter()
    {
        Hovered = this;
        transform.position += new Vector3(0, 0.2f, 0);
        InfoPanel.Show(this);
        if (Pathing.LastCalculatedPath != null)
            foreach (var tile in Pathing.LastCalculatedPath)
                tile.SetIndicator(TargetIndicator.Path);
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        Hovered = null;
        transform.position -= new Vector3(0, 0.2f, 0);
        // ClearGhost();
        InfoPanel.Hide();
        if (Pathing.LastCalculatedPath != null)
            foreach (var tile in Pathing.LastCalculatedPath)
                tile.SetIndicator(TargetIndicator.Valid);
    }

    public event Action<Tile> OnClickedValidTarget;

    void OnMouseDown()
    {
        if (CurrentIndicator == TargetIndicator.Path || CurrentIndicator == TargetIndicator.Valid)
        {
            OnClickedValidTarget?.Invoke(this);
        }
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
        start.Pathing.LastCalculatedPath = null;

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
    public void SetIndicator(TargetIndicator state)
    {
        if (state == TargetIndicator.Hidden)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            Pathing = new PathingData();
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            var color = Color.black;
            switch (state)
            {
                case TargetIndicator.Invalid: color = new Color(1, 0, 0, 0.8f); break;
                case TargetIndicator.Path: color = new Color(0, 1, 0, 0.8f); break;
                case TargetIndicator.Valid: color = new Color(0, 0, 1, 0.8f); break;
            }
            transform.GetChild(0).GetComponent<Renderer>().material.color = color;
        }
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
    }

    int FaceToRot(Facing face)
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

    Facing RotToFace(float rot)
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

    public void RotateUnit(Facing targetFacing)
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