using System;
using System.Collections;
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

    protected override Tile[] OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        var unit = SourceTile?.Unit;
        if (unit == null)
            return new Tile[0];
        originalFacing = SourceTile.Facing;
        return Settings.Map.ShowRange(SourceTile, minRange, maxRange, TargetFilter, RangeFilter, player);
    }

    protected override IEnumerator OnPreview()
    {
        targetFacing = Tile.GetFacing(SourceTile, TargetTile);
        SourceTile.RotateUnit(targetFacing);
        yield return null;
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