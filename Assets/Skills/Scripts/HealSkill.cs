using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Heal")]
public class HealSkill : Skill
{
    public TargetType Filter;
    public int MinRange;
    public int MaxRange;
    public RangeType Type;

    public float Heal;

    Unit previewUnit;

    protected override Tile[] OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        if ((Filter & TargetType.Empty) > 0) return new Tile[0];
        return Settings.Map.ShowRange(SourceTile, MinRange, MaxRange, Filter, Type, player);
    }

    protected override void OnPreview()
    {
        previewUnit = TargetTile.Unit;
        previewUnit.Damage(-Heal, true);
    }

    public override void OnUse()
    {
        previewUnit.Damage(-Heal, false);
    }

    protected override void OnCancel()
    {
        TargetTile.Unit.Damage(Heal, true);
    }
}