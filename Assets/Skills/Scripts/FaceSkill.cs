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

    protected override bool OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        var unit = SourceTile?.Unit;
        if (unit == null)
            return false;
        originalFacing = SourceTile.Facing;
        return Settings.Map.ShowRange(SourceTile, minRange, maxRange, TargetFilter, RangeFilter, player) > 0;
    }

    protected override void OnPreview()
    {
        targetFacing = Tile.GetFacing(SourceTile, TargetTile);
        SourceTile.RotateUnit(targetFacing);
    }

    public override void OnUse()
    {
        SourceTile.RotateUnit(targetFacing);
    }

    protected override void OnCancel()
    {
        SourceTile.RotateUnit(originalFacing);
    }
}