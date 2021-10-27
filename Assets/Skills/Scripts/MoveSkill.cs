using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class MoveSkill : Skill, ICancelable
{
    public int Range;
    public RangeFilter RangeType;

    Unit unit;
    Facing rotation;

    public override string Description => $"{(RangeType == RangeFilter.Pathfinding ? "Move" : "Jump")} {Range}";

    protected override bool AutoSelectOnOnePossibleTile => false;

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (SourceTile?.Unit == null)
            return new Tile[0];
        unit = SourceTile.Unit;
        rotation = SourceTile.Facing;
        return map.ShowRange(SourceTile, 0, Range, filter, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return unit.Move(SourceTile, TargetTile, RangeType);
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
        unit.MoveInstant(TargetTile, SourceTile);
        SourceTile.Face(rotation);
    }
}