using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlaceSkill : Skill
{
    [SerializeField] public Unit reference;

    public override string Description => null;

    public override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        return new SpawnPatternBuilder().Patterns(ctx.Source.Map, ctx.SourceTile, ctx.Player, SelectionFilters.Empty);
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();

        var facing = ctx.OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        ctx.OutTile.Place(reference, facing);
        reference.Player = ctx.Player;
        yield return null;
    }
}