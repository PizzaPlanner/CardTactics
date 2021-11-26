using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MoveSkill : Skill
{
    public int Range;
    public RangeFilter RangeType;

    public override string Description => $"{(RangeType == RangeFilter.Path ? "Move" : "Jump")} {Range}";

    public override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
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
            }.Patterns(ctx.Source.Map, ctx.SourceTile, filter);
        else
            return new RangePatternBuilder { MinRange = 1, MaxRange = Range }.Patterns(ctx.Source.Map, ctx.SourceTile, filter);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        if (ctx.SkillTarget is PathPattern pp)
            ctx.OutTile = pp.Target;
        else if (ctx.SkillTarget is SingleTilePattern stp)
            ctx.OutTile = stp.Target;
        yield return ctx.SourceTile.Unit.Move(ctx.SourceTile, ctx.SkillTarget, ctx.OutTile, RangeType);
    }

}