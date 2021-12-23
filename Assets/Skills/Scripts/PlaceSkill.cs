using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlaceSkill : ISkill
{
    [SerializeField] public Unit reference;

    public string Description => null;

    public bool AutoSelectOnOnePossibleTile => true;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        return new SpawnPatternBuilder { UnitToSpawn = reference }.Patterns(ctx.Map, ctx.SourceTile, SelectionFilters.Empty);
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        var facing = ctx.OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        ctx.OutTile.Place(reference, facing);
        reference.Spawn();
        yield return null;
    }

    public void Serialize(Mirror.NetworkWriter writer) { }
    public ISkill Deserialize(Mirror.NetworkReader reader) { return this; }
}