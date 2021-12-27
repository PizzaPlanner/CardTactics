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
        return new CrosshairPatternBuilder { Length = MaxRange, Offset = 1 }.Patterns(Map.Instance, ctx.SourceTile, filter);
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

    public void ExecuteServer(SkillExecuteContext ctx)
    {
        ctx.SourceTile.Unit.MoveInstant(ctx.SourceTile, ctx.OutTile);
    }

    public IEnumerator ExecuteClient(SkillExecuteContext ctx)
    {
        Animator sourceAnimator = ctx.Unit.GetComponentInChildren<Animator>();
        if (sourceAnimator == null) yield break;
        sourceAnimator.SetBool("Moving", true);
        yield return ctx.SourceTile.Unit.Move(ctx.SourceTile, new EmptyPattern(), ctx.OutTile, RangeType);
        sourceAnimator.SetBool("Moving", false);
    }

    public void Serialize(Mirror.NetworkWriter writer)
    {
        writer.Write((int)RangeType);
        writer.Write(MaxRange);
    }
    public ISkill Deserialize(Mirror.NetworkReader reader)
    {
        RangeType = (RangeFilter)reader.Read<int>();
        MaxRange = reader.Read<int>();
        return this;
    }
}