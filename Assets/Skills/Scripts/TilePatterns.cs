using System.Collections.Generic;
using System.Linq;
using Mirror;
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

    public ITilePattern Deserialize(NetworkReader reader)
    {
        var pattern = new SingleTilePattern
        {
            Target = reader.Read<Tile>(),
            Selectable = reader.Read<bool>(),
            Tiles = reader.Read<Tile[]>()
        };
        return pattern;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(Target);
        writer.Write(Selectable);
        writer.Write(Tiles.ToArray());
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

    public ITilePattern Deserialize(NetworkReader reader)
    {
        var pattern = new LinePattern
        {
            Offset = reader.Read<Vector2Int>(),
            Length = reader.Read<int>(),
            Selectable = reader.Read<bool>(),
            TilesByDistance = reader.Read<Tile[]>()
        };
        return pattern;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Length);
        writer.Write(Selectable);
        writer.Write(TilesByDistance);
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

    public ITilePattern Deserialize(NetworkReader reader)
    {
        var pattern = new RectPattern
        {
            Offset = reader.Read<Vector2Int>(),
            Size = reader.Read<Vector2Int>(),
            Selectable = reader.Read<bool>(),
            Tiles = reader.Read<Tile[]>()
        };
        return pattern;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Size);
        writer.Write(Selectable);
        writer.Write(Tiles.ToArray());
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

    public ITilePattern Deserialize(NetworkReader reader)
    {
        var pattern = new PathPattern
        {
            MinLength = reader.Read<int>(),
            MaxLength = reader.Read<int>(),
            Target = reader.Read<Tile>(),
            Selectable = reader.Read<bool>(),
            Tiles = reader.Read<Tile[]>()
        };
        return pattern;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(MinLength);
        writer.Write(MaxLength);
        writer.Write(Target);
        writer.Write(Selectable);
        writer.Write(Tiles.ToArray());
    }
}

public struct EmptyPattern : ITilePattern
{
    public bool Selectable => false;
    public IEnumerable<Tile> Tiles => new Tile[0];
    public ITilePattern Build(Map map, Tile origin, SelectionFilter filter) { return this; }

    public ITilePattern Deserialize(NetworkReader reader)
    {
        return new EmptyPattern();
    }

    public void Serialize(NetworkWriter writer)
    {

    }
}

public interface ITilePattern
{
    bool Selectable { get; }
    ITilePattern Build(Map map, Tile origin, SelectionFilter filter);
    IEnumerable<Tile> Tiles { get; }
    IEnumerable<Tile> UnselectableTiles => new Tile[0];

    void Serialize(NetworkWriter writer);
    ITilePattern Deserialize(NetworkReader reader);
}

public interface ITilePatternBuilder
{
    IEnumerable<ITilePattern> Patterns(Map map, Tile origin, SelectionFilter filter);
}

public struct TilePatternMessage : NetworkMessage
{
    public ITilePattern[] Patterns;
}

public static class TilePatternSerializeHelper
{
    public static void WritePattern(this NetworkWriter writer, ITilePattern pattern)
    {
        writer.WriteString(pattern.GetType().Name);
        writer.WriteString(pattern.GetType().Assembly.GetName().Name);
        pattern.Serialize(writer);
    }
    public static ITilePattern ReadPattern(this NetworkReader reader)
    {
        var type = reader.ReadString();
        var asm = reader.ReadString();
        var inst = (ITilePattern)System.Activator.CreateInstance(asm, type).Unwrap();
        return inst.Deserialize(reader);
    }

    public static void WriteTile(this NetworkWriter writer, Tile tile) => writer.WriteVector3Int(tile != null ? tile.Position : new Vector3Int(0, 0, int.MaxValue));
    public static Tile ReadTile(this NetworkReader reader)
    {
        var pos = reader.Read<Vector3Int>();
        if (pos.z == int.MaxValue)
            return null;
        return Map.FindObjectOfType<Map>().VisibleTiles.First(x => x.Position == pos);
    }

    public static void WriteTileArray(this NetworkWriter writer, Tile[] tiles)
    {
        writer.Write(tiles.Length);
        foreach (var tile in tiles)
            writer.Write(tile);
    }

    public static Tile[] ReadTileArray(this NetworkReader reader)
    {
        var tiles = new List<Tile>();
        var number = reader.Read<int>();
        for (int i = 0; i < number; i++)
            tiles.Add(reader.Read<Tile>());
        return tiles.ToArray();
    }
}