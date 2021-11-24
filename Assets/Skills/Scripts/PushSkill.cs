using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PushSkill : Skill
{
    public int MaxRange;
    public int Tiles;

    public override string Description => $"Push {Tiles}";

    public override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        var filter = SelectionFilters.AnyUnit;
        if (ctx.SourceTile.Unit == null)
            return new ITilePattern[0];
        return new RangePatternBuilder
        {
            MinRange = 1,
            MaxRange = MaxRange
        }.Patterns(ctx.Source.Map, ctx.SourceTile, ctx.Player, filter);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
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