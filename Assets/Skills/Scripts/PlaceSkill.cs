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

    public IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        return new SpawnPatternBuilder { UnitToSpawn = reference }.Patterns(ctx.Map, ctx.SourceTile, SelectionFilters.Empty);
    }

    public IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();

        var facing = ctx.OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        ctx.OutTile.Place(reference, facing);
        yield return null;
    }
}