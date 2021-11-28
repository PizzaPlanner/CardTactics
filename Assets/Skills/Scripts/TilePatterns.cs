using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public struct CrosshairPatternBuilder : ITilePatternBuilder
{
    public int Offset;
    public int Length;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter)
    {
        yield return new LinePattern { Offset = new Vector2Int(Offset, 0), Length = Length }.Build(map, origin, filter);
        yield return new LinePattern { Offset = new Vector2Int(-Offset, 0), Length = Length }.Build(map, origin, filter);
        yield return new LinePattern { Offset = new Vector2Int(0, Offset), Length = Length }.Build(map, origin, filter);
        yield return new LinePattern { Offset = new Vector2Int(0, -Offset), Length = Length }.Build(map, origin, filter);
    }
}

public struct GlobalPatternBuilder : ITilePatternBuilder
{
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter)
    {
        return map.VisibleTiles.Where(x => map.TileMatches(x, filter, origin.Unit)).Select(x => new SingleTilePattern { Target = x }.Build(map, origin, filter));
    }
}

public struct SpawnPatternBuilder : ITilePatternBuilder
{
    public Unit UnitToSpawn;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter)
    {
        if (UnitToSpawn.Player.Team == -1)
            return new ITilePattern[0];
        return map.GetSpawnTiles(filter, UnitToSpawn.Player.Team).Select(x => new SingleTilePattern { Target = x }.Build(map, x, filter));
    }
}

public struct PathPatternBuilder : ITilePatternBuilder
{
    public int MinRange, MaxRange;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter)
    {
        var min = MinRange;
        var max = MaxRange;
        return map.VisibleTiles
        .Where(x => Tile.Distance(origin, x) <= max)
        .Select(x => new PathPattern { MinLength = min, MaxLength = max, Target = x }.Build(map, origin, filter));
    }
}

public struct RangePatternBuilder : ITilePatternBuilder
{
    public int MinRange;
    public int MaxRange;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter)
    {
        var min = MinRange;
        var max = MaxRange;
        return map.VisibleTiles
        .Where(x => Tile.Distance(origin, x) >= min && Tile.Distance(origin, x) <= max)
        .Select(x => new SingleTilePattern { Target = x }.Build(map, origin, filter));
    }
}

public struct SingleTilePattern : ITilePattern
{
    public Tile Target;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles { get; private set; }
    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter)
    {
        Tiles = new Tile[] { Target };
        Selectable = map.TileMatches(Target, filter, origin.Unit);
        return this;
    }
}

public struct LinePattern : ITilePattern
{
    public Vector2Int Offset;
    public int Length;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles => TilesByDistance;
    public Tile[] TilesByDistance { get; private set; }
    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter)
    {
        int sign(int x) => x == 0 ? 0 : (int)Mathf.Sign(x);
        var tiles = new List<Tile>();
        var start = Offset + new Vector2Int(origin.Position.x, origin.Position.y);
        var end = start + new Vector2Int(sign(Offset.x) * (Length - 1), sign(Offset.y) * (Length - 1));
        tiles = map.VisibleTiles.Where(t =>
        t.Position.x >= Mathf.Min(start.x, end.x) &&
        t.Position.x <= Mathf.Max(start.x, end.x) &&
        t.Position.y >= Mathf.Min(start.y, end.y) &&
        t.Position.y <= Mathf.Max(start.y, end.y)).ToList();
        tiles.Sort((x, y) => Tile.Distance(origin, x).CompareTo(Tile.Distance(origin, y)));
        TilesByDistance = tiles.ToArray();
        Selectable = Tiles.Count(x => map.TileMatches(x, filter, origin.Unit)) > 0;
        return this;
    }
}

public struct RectPattern : ITilePattern
{
    public Vector2Int Offset, Size;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles { get; private set; }
    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter)
    {
        var min = Offset + new Vector2Int(origin.Position.x, origin.Position.y);
        var max = min + Size;
        Tiles = map.VisibleTiles.Where(t => t.Position.x >= min.x && t.Position.x < max.x && t.Position.y >= min.y && t.Position.y < max.y);
        if (Tiles.Count(x => map.TileMatches(x, filter, origin.Unit)) == 0)
            return new EmptyPattern();
        Selectable = true;
        return this;
    }
}

public struct PathPattern : ITilePattern
{
    public int MinLength, MaxLength;
    public Tile Target;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles { get; private set; }
    public IEnumerable<Tile> UnselectableTiles { get { var tgt = Target; return Tiles.Where(x => x != tgt); } }

    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter)
    {
        var length = MaxLength;
        var validTiles = map.VisibleTiles.Where(x => Tile.Distance(origin, x) <= length && map.TileMatches(x, filter, origin.Unit)).ToList();
        if (!Tile.FindPath(origin, Target, length, validTiles))
            return new EmptyPattern().Build(map, origin, filter);

        Tiles = Target.Pathing.LastCalculatedPath;
        Selectable = Tiles.Count() > MinLength;
        return this;
    }
}

public struct EmptyPattern : ITilePattern
{
    public bool Selectable => false;
    public IEnumerable<Tile> Tiles => new Tile[0];
    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter) { return this; }
}

public interface ITilePattern
{
    bool Selectable { get; }
    ITilePattern Build(Map map, Tile origin, SelectionFilter filter);
    IEnumerable<Tile> Tiles { get; }
    IEnumerable<Tile> UnselectableTiles => new Tile[0];
}

public interface ITilePatternBuilder
{
    IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter);
}