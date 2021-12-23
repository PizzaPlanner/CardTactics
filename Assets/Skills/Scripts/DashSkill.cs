using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DashSkill : ISkill
{
    public int MaxRange;
    public RangeFilter RangeType;

    public string Description => $"Dash {MaxRange}";

    public bool AutoSelectOnOnePossibleTile => false;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (ctx.SourceTile.Unit == null)
            return new ITilePattern[0];
        return new CrosshairPatternBuilder { Length = MaxRange, Offset = 1 }.Patterns(ctx.Map, ctx.SourceTile, filter);
    }

    public Tile GetOutTile(SkillPlanContext ctx)
    {
        var tilesByDistance = ((LinePattern)ctx.SkillTarget).TilesByDistance;
        var prevTile = ctx.SourceTile;
        foreach (var tile in tilesByDistance)
        {
            if (tile.Unit != null)
                break;
            prevTile = tile;
        }
        return prevTile;
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        yield return ctx.Unit.Move(ctx.SourceTile, new EmptyPattern(), ctx.OutTile, RangeType);
    }

    public void Serialize(Mirror.NetworkWriter writer) { }
    public ISkill Deserialize(Mirror.NetworkReader reader) { return this; }
}