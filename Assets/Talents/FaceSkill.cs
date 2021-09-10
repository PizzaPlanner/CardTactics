using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Face")]
public class FaceSkill : Skill
{
    public TargetType TargetFilter;
    public RangeType RangeFilter;
    public int minRange;
    public int maxRange;

    Tile originalTile;
    Facing originalFacing;
    Facing targetFacing;

    protected override bool OnActivate(int player, Card card, Tile selection)
    {
        var unit = selection?.Unit;
        if (unit == null)
            return false;
        originalTile = selection;
        originalFacing = selection.Facing;
        FindObjectOfType<Board>().ShowRange(selection, minRange, maxRange, TargetFilter, RangeFilter);
        return true;
    }

    protected override void OnPreview()
    {
        targetFacing = Tile.GetFacing(OriginTile, PreviewTile);
        originalTile.RotateUnit(targetFacing);
    }

    public override void OnUse()
    {
        originalTile.RotateUnit(targetFacing);
    }

    protected override void OnCancel()
    {
        originalTile.RotateUnit(originalFacing);
    }
}