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

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
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
            }.Patterns(Map.Instance, ctx.SourceTile, filter);
        else
            return new RangePatternBuilder { MinRange = 1, MaxRange = Range }.Patterns(Map.Instance, ctx.SourceTile, filter);
    }

    public Tile GetOutTile(SkillPlanContext ctx)
    {
        if (ctx.SkillTarget is PathPattern pp)
            return pp.Target;
        else if (ctx.SkillTarget is SingleTilePattern stp)
            return stp.Target;
        return null;
    }

    public void ExecuteServer(SkillExecuteContext ctx)
    {
        ctx.SourceTile.Unit.MoveInstant(ctx.SourceTile, ctx.OutTile);
    }

    public IEnumerator ExecuteClient(SkillExecuteContext ctx)
    {
        Animator sourceAnimator = ctx.SourceTile.Unit.GetComponentInChildren<Animator>();
        if (sourceAnimator == null) yield break;
        sourceAnimator.SetBool("Moving", true);
        yield return ctx.SourceTile.Unit.Move(ctx.SourceTile, ctx.SkillTarget, ctx.OutTile, RangeType);
        sourceAnimator.SetBool("Moving", false);
    }

    public void Serialize(Mirror.NetworkWriter writer)
    {
        writer.Write(Range);
        writer.Write((int)RangeType);
    }
    public ISkill Deserialize(Mirror.NetworkReader reader)
    {
        Range = reader.Read<int>();
        RangeType = (RangeFilter)reader.Read<int>();
        return this;
    }

}