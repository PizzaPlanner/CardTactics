using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Move")]
public class MoveSkill : Skill
{
    public int Range;
    public RangeType RangeType;

    Unit unit;
    Facing rotation;

    protected override bool OnActivate(int player, Card card)
    {
        if (OriginTile?.Unit == null)
            return false;
        unit = OriginTile.Unit;
        rotation = OriginTile.Facing;
        return FindObjectOfType<Board>().ShowRange(OriginTile, 0, Range, TargetType.Empty, RangeType) > 0;
    }

    protected override void OnPreview()
    {
        unit.Move(OriginTile, PreviewTile);
    }

    public override void OnUse()
    {
    }

    protected override void OnCancel()
    {
        unit.Move(PreviewTile, OriginTile);
        OriginTile.RotateUnit(rotation);
    }
}