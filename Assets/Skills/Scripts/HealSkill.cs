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

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        if ((Filter.Tile & TileFilter.Empty) > 0) return new ITilePattern[0];
        return new RangePatternBuilder
        {
            MinRange = MinRange,
            MaxRange = MaxRange
        }.Patterns(ctx.Map, ctx.SourceTile, Filter);
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        ctx.OutTile.Unit.Damage(-Heal, ctx.OriginTile.Unit);
        yield return null;
    }

    public IEnumerator ExecuteClient(SkillExecuteContext ctx)
    {
        if (GameSettings.Instance.UseAnimations)
        {
            ctx.SourceTile.Unit.GetComponentInChildren<Animator>().SetTrigger("Cast");
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Serialize(Mirror.NetworkWriter writer) { }
    public ISkill Deserialize(Mirror.NetworkReader reader) { return this; }
}