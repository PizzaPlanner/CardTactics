using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Move")]
public class MoveSkill : Skill
{
    public int Range;
    public RangeType RangeType;

    Unit unit;
    Facing rotation;

    protected override Tile[] OnActivate(GameSettings settings, PlayerInfo player, Card card)
    {
        if (SourceTile?.Unit == null)
            return new Tile[0];
        unit = SourceTile.Unit;
        rotation = SourceTile.Facing;
        return settings.Map.ShowRange(SourceTile, 0, Range, TargetType.Empty, RangeType, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return unit.Move(SourceTile, TargetTile, RangeType);
    }

    protected override void OnUse()
    {
    }

    protected override void OnCancel()
    {
        unit.MoveInstant(TargetTile, SourceTile);
        SourceTile.Face(rotation);
    }
}