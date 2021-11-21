using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MoveSkill : Skill, ICancelable
{
    public int Range;
    public RangeFilter RangeType;

    Unit unit;
    Facing rotation;

    public override string Description => $"{(RangeType == RangeFilter.Path ? "Move" : "Jump")} {Range}";

    protected override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (SourceTile?.Unit == null)
            return new ITilePattern[0];
        unit = SourceTile.Unit;
        rotation = SourceTile.Facing;
        if (RangeType == RangeFilter.Path)
            return new PathPatternBuilder { MinRange = 1, MaxRange = Range }.Patterns(map, SourceTile, player, filter);
        else
            return new RangePatternBuilder { MinRange = 1, MaxRange = Range }.Patterns(map, SourceTile, player, filter);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        if (targetPattern is PathPattern pp)
            OutTile = pp.Target;
        else if (targetPattern is SingleTilePattern stp)
            OutTile = stp.Target;
        yield return unit.Move(SourceTile, targetPattern, OutTile, RangeType);
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
        unit.MoveInstant(OutTile, SourceTile);
        SourceTile.Face(rotation);
    }
}