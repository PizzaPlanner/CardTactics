using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Attack")]
public class AttackSkill : Skill
{
    public TargetType Filter;
    public int MinRange;
    public int MaxRange;
    public RangeType Type;

    public float Damage;

    Unit previewUnit;

    protected override Tile[] OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        if ((Filter & TargetType.Empty) > 0) return new Tile[0];
        return Settings.Map.ShowRange(SourceTile, MinRange, MaxRange, Filter, Type, player);
    }

    protected override void OnPreview()
    {
        previewUnit = TargetTile.Unit;
        previewUnit.Damage(Damage, true);
        if (previewUnit.LifePreview <= 0)
        {
            previewUnit.gameObject.SetActive(false);
            TargetTile.Unit = null;
        }
    }

    public override void OnUse()
    {
        previewUnit.Damage(Damage, false);
    }

    protected override void OnCancel()
    {
        if (previewUnit.LifePreview <= 0)
        {
            previewUnit.gameObject.SetActive(true);
            TargetTile.Unit = previewUnit;
        }
        TargetTile.Unit.Damage(-Damage, true);
    }
}