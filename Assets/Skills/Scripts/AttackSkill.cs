using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttackSkill : Skill
{
    [ValidateInput("@((int)$value.Tile & (int)TileFilter.Empty) == 0")]
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;
    [SerializeField] AudioClip Sound;
    [SerializeField] bool confirmOnePossibleTile;
    public float Damage;

    public override string Description => $"Attack {Damage} {(MaxRange > 1 ? $"(Range {MaxRange})" : "")}";
    public override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        return new RangePatternBuilder
        {
            MinRange = MinRange,
            MaxRange = MaxRange
        }.Patterns(ctx.Source.Map, ctx.SourceTile, ctx.Player, Filter);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();
        Animator sourceAnimator = ctx.SourceTile.Unit.GetComponentInChildren<Animator>();
        Animator targetAnimator = ctx.OutTile.Unit.GetComponentInChildren<Animator>();
        ctx.SourceTile.Face(Tile.GetFacing(ctx.SourceTile, ctx.OutTile));
        if (GameSettings.Instance.UseAnimations)
        {
            if (sourceAnimator != null)
            {
                sourceAnimator.SetInteger("Action", 1);
                sourceAnimator.SetTrigger("AttackTrigger");
            }
            if (Sound != null)
                ctx.SourceTile.Unit.GetComponent<AudioSource>().PlayOneShot(Sound);
            yield return new WaitForSeconds(0.3f);
            if (targetAnimator != null)
            {
                targetAnimator.SetInteger("Action", 1);
                targetAnimator.SetTrigger("GetHitTrigger");
            }
        }
        ctx.OutTile.Unit.Damage(Damage, ctx.OriginTile.Unit);
    }
}