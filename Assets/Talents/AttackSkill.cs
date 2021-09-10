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

    protected override bool OnActivate(int player, Card card)
    {
        if ((Filter & TargetType.Empty) > 0) return false;
        return FindObjectOfType<Board>().ShowRange(OriginTile, MinRange, MaxRange, Filter, Type) > 0;
    }

    protected override void OnPreview()
    {
        previewUnit = PreviewTile.Unit;
        previewUnit.Damage(Damage, true);
        if (previewUnit.LifePreview <= 0)
        {
            previewUnit.gameObject.SetActive(false);
            PreviewTile.Unit = null;
        }
    }

    public override void OnUse()
    {
        previewUnit.Damage(Damage, false);
        if (previewUnit.Life <= 0)
        {
            Destroy(previewUnit.gameObject);
            PreviewTile.Unit = null;
        }
    }

    protected override void OnCancel()
    {
        if (previewUnit.LifePreview <= 0)
        {
            previewUnit.gameObject.SetActive(true);
            PreviewTile.Unit = previewUnit;
        }
        PreviewTile.Unit.Damage(-Damage, true);
    }
}