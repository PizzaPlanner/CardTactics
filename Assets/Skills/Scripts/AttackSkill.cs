using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[CreateAssetMenu(menuName = "Skills/Attack")]
public class AttackSkill : Skill
{
    public TargetType Filter;
    public int MinRange;
    public int MaxRange;
    public RangeType Type;

    public float Damage;

    Unit previewUnit;
    Facing previewFacing;

    protected override Tile[] OnActivate(GameSettings settings, GameSettings.PlayerInfo player, Card card)
    {
        if ((Filter & TargetType.Empty) > 0) return new Tile[0];
        return settings.Map.ShowRange(SourceTile, MinRange, MaxRange, Filter, Type, player);
    }

    protected override IEnumerator OnPreview()
    {
        previewFacing = SourceTile.Facing;
        SourceTile.Face(Tile.GetFacing(SourceTile, TargetTile));
        SourceTile.Unit.GetComponentInChildren<Animator>().SetInteger("Action", 1);
        SourceTile.Unit.GetComponentInChildren<Animator>().SetTrigger("AttackTrigger");
        yield return new WaitForSeconds(0.3f);
        previewUnit = TargetTile.Unit;
        previewUnit.Damage(Damage, true);
        previewUnit.GetComponentInChildren<Animator>().SetInteger("Action", 1);
        previewUnit.GetComponentInChildren<Animator>().SetTrigger("GetHitTrigger");
        if (previewUnit.LifePreview <= 0)
        {
            yield return new WaitForSeconds(0.5f);
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
        SourceTile.Face(previewFacing);
    }
}