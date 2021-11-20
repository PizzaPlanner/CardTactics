using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PushSkill : Skill
{
    public int MaxRange;
    public int Tiles;

    public override string Description => $"Push {Tiles}";

    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        var filter = SelectionFilters.AnyUnit;
        if (SourceTile.Unit == null)
            return new ITilePattern[0];
        return new RangePatternBuilder { MinRange = 1, MaxRange = MaxRange }.Patterns(map, SourceTile, player, filter);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        var tilesByDistance = ((LinePattern)targetPattern).TilesByDistance;
        var prevTile = SourceTile;
        foreach (var tile in tilesByDistance)
        {
            if (tile.Unit != null)
                break;
            yield return prevTile.Unit.Move(prevTile, new EmptyPattern(), tile, RangeFilter.Pathfinding);
            prevTile = tile;
            OutTile = tile;
        }
    }

    protected override void OnUse()
    {
    }
}