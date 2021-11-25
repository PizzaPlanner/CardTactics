using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class HealSkill : Skill
{
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;

    public float Heal;

    public override string Description => $"Heal {Heal}";
    public override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        if ((Filter.Tile & TileFilter.Empty) > 0) return new ITilePattern[0];
        return new RangePatternBuilder
        {
            MinRange = MinRange,
            MaxRange = MaxRange
        }.Patterns(ctx.Source.Map, ctx.SourceTile, ctx.Player, Filter);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();
        if (GameSettings.Instance.UseAnimations)
        {
            ctx.SourceTile.Unit.GetComponentInChildren<Animator>().SetTrigger("Cast");
            yield return new WaitForSeconds(0.5f);
        }
        ctx.OutTile.Unit.Damage(-Heal, ctx.OriginTile.Unit);
    }
}