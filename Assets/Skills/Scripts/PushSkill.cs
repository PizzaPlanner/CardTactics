using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PushSkill : ISkill
{
    public int MaxRange;
    public int Tiles;

    public string Description => $"Push {Tiles}";

    public bool AutoSelectOnOnePossibleTile => true;

    public IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        var filter = SelectionFilters.AnyUnit;
        if (ctx.SourceTile.Unit == null)
            return new ITilePattern[0];
        return new RangePatternBuilder
        {
            MinRange = 1,
            MaxRange = MaxRange
        }.Patterns(ctx.Map, ctx.SourceTile, filter);
    }

    public IEnumerator OnPreview(SkillContext ctx)
    {
        var tilesByDistance = ((LinePattern)ctx.SkillTarget).TilesByDistance;
        var prevTile = ctx.SourceTile;
        foreach (var tile in tilesByDistance)
        {
            if (tile.Unit != null)
                break;
            yield return prevTile.Unit.Move(prevTile, new EmptyPattern(), tile, RangeFilter.Path);
            prevTile = tile;
            ctx.OutTile = tile;
        }
    }
}