using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Move")]
public class MoveSkill : Skill
{
    public int Range;
    public RangeType RangeType;

    Unit unit;
    Facing rotation;
    Tile origin;

    protected override bool OnActivate(int player, Card card, Tile selection)
    {
        if (selection?.Unit == null)
            return false;
        origin = selection;
        unit = selection.Unit;
        rotation = selection.Facing;
        FindObjectOfType<Board>().ShowRange(selection, 0, Range, TargetType.Empty, RangeType);
        return true;
    }

    protected override void OnPreview()
    {
        unit.Move(origin, PreviewTile);
    }

    public override void OnUse()
    {
    }

    protected override void OnCancel()
    {
        unit.Move(PreviewTile, origin);
        origin.RotateUnit(rotation);
    }
}