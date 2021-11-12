using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct CrosshairPatternBuilder : ITilePatternBuilder
{
    public int Offset;
    public int Length;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        yield return new LinePattern { Offset = new Vector2Int(Offset, 0), Size = new Vector2Int(Length, 0) }.Build(map, origin, player, filter);
        yield return new LinePattern { Offset = new Vector2Int(-Offset, 0), Size = new Vector2Int(-Length, 0) }.Build(map, origin, player, filter);
        yield return new LinePattern { Offset = new Vector2Int(0, Offset), Size = new Vector2Int(0, Length) }.Build(map, origin, player, filter);
        yield return new LinePattern { Offset = new Vector2Int(0, -Offset), Size = new Vector2Int(0, -Length) }.Build(map, origin, player, filter);
    }
}

public struct GlobalPatternBuilder : ITilePatternBuilder
{
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        return map.VisibleTiles.Where(x => map.TileMatches(x, filter, player)).Select(x => new SingleTilePattern { Target = x }.Build(map, origin, player, filter));
    }
}

public struct SpawnPatternBuilder : ITilePatternBuilder
{
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        return map.GetSpawnTiles(filter, player).Select(x => new SingleTilePattern { Target = x }.Build(map, origin, player, filter));
    }
}

public struct PathPatternBuilder : ITilePatternBuilder
{
    public int MinRange, MaxRange;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        var min = MinRange;
        var max = MaxRange;
        return map.VisibleTiles
        .Where(x => Tile.Distance(origin, x) <= max)
        .Select(x => new PathPattern { MinLength = min, MaxLength = max, Target = x }.Build(map, origin, player, filter));
    }
}

public struct RangePatternBuilder : ITilePatternBuilder
{
    public int MinRange;
    public int MaxRange;
    public IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        var min = MinRange;
        var max = MaxRange;
        return map.VisibleTiles
        .Where(x => Tile.Distance(origin, x) >= min && Tile.Distance(origin, x) <= max)
        .Select(x => new SingleTilePattern { Target = x }.Build(map, origin, player, filter));
    }
}

public struct SingleTilePattern : ITilePattern
{
    public Tile Target;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles { get; private set; }
    public ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        Tiles = new Tile[] { Target };
        Selectable = map.TileMatches(Target, filter, player);

        return this;
    }

    public bool Valid(Map map, SelectionFilter filter, PlayerInfo player)
    {
        // Make sure to call TileMatches on every tile in pattern!
        return Tiles.Count(x => map.TileMatches(x, filter, player)) > 0;
    }
}

public struct LinePattern : ITilePattern
{
    public Vector2Int Offset, Size;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles => TilesByDistance;
    public Tile[] TilesByDistance { get; private set; }
    public ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        var tiles = new List<Tile>();
        var start = Offset + new Vector2Int(origin.Position.x, origin.Position.y);
        var end = start + Size;
        tiles = map.VisibleTiles.Where(t =>
        t.Position.x >= Mathf.Min(start.x, end.x) &&
        t.Position.x <= Mathf.Max(start.x, end.x) &&
        t.Position.y >= Mathf.Min(start.y, end.y) &&
        t.Position.y <= Mathf.Max(start.y, end.y)).ToList();
        tiles.Sort((x, y) => Tile.Distance(origin, x).CompareTo(Tile.Distance(origin, y)));
        TilesByDistance = tiles.ToArray();
        Selectable = Tiles.Count(x => map.TileMatches(x, filter, player)) > 0;
        return this;
    }
}

public struct RectPattern : ITilePattern
{
    public Vector2Int Offset, Size;

    public bool Selectable { get; private set; }
    public IEnumerable<Tile> Tiles { get; private set; }
    public ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        var min = Offset + new Vector2Int(origin.Position.x, origin.Position.y);
        var max = min + Size;
        Tiles = map.VisibleTiles.Where(t => t.Position.x >= min.x && t.Position.x <= max.x && t.Position.y >= min.y && t.Position.y <= max.y);
        if (Tiles.Count(x => map.TileMatches(x, filter, player)) == 0)
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

    public ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter)
    {
        var length = MaxLength;
        var validTiles = map.VisibleTiles.Where(x => Tile.Distance(origin, x) <= length && map.TileMatches(x, filter, player)).ToList();
        if (!Tile.FindPath(origin, Target, length, validTiles))
            return new EmptyPattern().Build(map, origin, player, filter);

        Tiles = Target.Pathing.LastCalculatedPath;
        Selectable = Tiles.Count() > MinLength;
        return this;
    }
}

public struct EmptyPattern : ITilePattern
{
    public bool Selectable => false;
    public IEnumerable<Tile> Tiles => new Tile[0];
    public ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter) { return this; }

    public bool Valid(Map map, SelectionFilter filter, PlayerInfo player) => false;
}

public interface ITilePattern
{
    bool Selectable { get; }
    ITilePattern Build(Map map, Tile origin, PlayerInfo player, SelectionFilter filter);
    IEnumerable<Tile> Tiles { get; }
    IEnumerable<Tile> UnselectableTiles => new Tile[0];
}

public interface ITilePatternBuilder
{
    IEnumerable<ITilePattern> Patterns(Map map, Tile origin, PlayerInfo player, SelectionFilter filter);
}