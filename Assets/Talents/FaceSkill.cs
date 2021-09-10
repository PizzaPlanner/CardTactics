using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Face")]
public class FaceSkill : Skill
{
    public TargetType TargetFilter;
    public RangeType RangeFilter;
    public int minRange;
    public int maxRange;

    Facing originalFacing;
    Facing targetFacing;

    protected override bool OnActivate(int player, Card card)
    {
        var unit = OriginTile?.Unit;
        if (unit == null)
            return false;
        originalFacing = OriginTile.Facing;
        return FindObjectOfType<Board>().ShowRange(OriginTile, minRange, maxRange, TargetFilter, RangeFilter) > 0;
    }

    protected override void OnPreview()
    {
        targetFacing = Tile.GetFacing(OriginTile, PreviewTile);
        OriginTile.RotateUnit(targetFacing);
    }

    public override void OnUse()
    {
        OriginTile.RotateUnit(targetFacing);
    }

    protected override void OnCancel()
    {
        OriginTile.RotateUnit(originalFacing);
    }
}