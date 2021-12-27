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
        }.Patterns(Map.Instance, ctx.SourceTile, Filter);
    }

    public void ExecuteServer(SkillExecuteContext ctx)
    {
        ctx.OutTile.Unit.Damage(-Heal, ctx.OriginTile.Unit);
    }

    public IEnumerator ExecuteClient(SkillExecuteContext ctx)
    {
        if (GameSettings.Instance.UseAnimations)
        {
            ctx.SourceTile.Unit.GetComponentInChildren<Animator>().SetTrigger("Cast");
            yield return new WaitForSeconds(0.5f);
            ctx.OutTile.Unit.UpdateDisplayLife = true;
        }
    }
    public void Serialize(Mirror.NetworkWriter writer)
    {
        writer.Write(Heal);
        writer.Write(MinRange);
        writer.Write(MaxRange);
        writer.Write((int)Filter.Unit);
        writer.Write((int)Filter.Tile);
        writer.Write((int)Filter.Range);
    }
    public ISkill Deserialize(Mirror.NetworkReader reader)
    {
        Heal = reader.Read<float>();
        MinRange = reader.Read<int>();
        MaxRange = reader.Read<int>();
        Filter = new SelectionFilter
        {
            Unit = (UnitFilter)reader.Read<int>(),
            Tile = (TileFilter)reader.Read<int>(),
            Range = (RangeFilter)reader.Read<int>()
        };
        return this;
    }
}