using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class HealSkill : ISkill
{
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;

    public float Heal;

    public string Description => $"Heal {Heal}";
    public bool AutoSelectOnOnePossibleTile => false;

    public IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        if ((Filter.Tile & TileFilter.Empty) > 0) return new ITilePattern[0];
        return new RangePatternBuilder
        {
            MinRange = MinRange,
            MaxRange = MaxRange
        }.Patterns(ctx.Map, ctx.SourceTile, Filter);
    }

    public IEnumerator OnPreview(SkillContext ctx)
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