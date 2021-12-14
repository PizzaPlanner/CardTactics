using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MoveSkill : ISkill
{
    public int Range;
    public RangeFilter RangeType;

    public string Description => $"{(RangeType == RangeFilter.Path ? "Move" : "Jump")} {Range}";

    public bool AutoSelectOnOnePossibleTile => false;

    public IEnumerable<ITilePattern> CreatePatterns(SkillContext ctx)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (ctx.SourceTile?.Unit == null)
            return new ITilePattern[0];
        if (RangeType == RangeFilter.Path)
            return new PathPatternBuilder
            {
                MinRange = 1,
                MaxRange = Range
            }.Patterns(ctx.Map, ctx.SourceTile, filter);
        else
            return new RangePatternBuilder { MinRange = 1, MaxRange = Range }.Patterns(ctx.Map, ctx.SourceTile, filter);
    }

    public Tile GetOutTile(SkillContext ctx)
    {
        if (ctx.SkillTarget is PathPattern pp)
            return pp.Target;
        else if (ctx.SkillTarget is SingleTilePattern stp)
            return stp.Target;
        return null;
    }

    public IEnumerator ExecuteServer(SkillContext ctx)
    {
        yield return ctx.SourceTile.Unit.Move(ctx.SourceTile, ctx.SkillTarget, ctx.OutTile, RangeType);
    }

    public IEnumerator ExecuteClient(SkillContext ctx)
    {
        yield return null;
    }

    public void Serialize(Mirror.NetworkWriter writer) { }
    public ISkill Deserialize(Mirror.NetworkReader reader) { return this; }

}