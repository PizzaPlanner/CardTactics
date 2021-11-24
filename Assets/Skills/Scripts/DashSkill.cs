using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DashSkill : Skill
{
    public int MaxRange;
    public RangeFilter RangeType;

    public override string Description => $"Dash {MaxRange}";

    public override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (ctx.SourceTile.Unit == null)
            return new ITilePattern[0];
        return new CrosshairPatternBuilder { Length = MaxRange, Offset = 1 }.Patterns(ctx.Source.Map, ctx.SourceTile, ctx.Player, filter);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        var tilesByDistance = ((LinePattern)ctx.SkillTarget).TilesByDistance;
        var prevTile = ctx.SourceTile;
        foreach (var tile in tilesByDistance)
        {
            if (tile.Unit != null)
                break;
            yield return prevTile.Unit.Move(prevTile, new EmptyPattern(), tile, RangeType);
            prevTile = tile;
            ctx.OutTile = tile;
        }
    }

}