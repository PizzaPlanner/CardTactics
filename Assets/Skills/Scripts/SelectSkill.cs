using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SelectSkill : Skill
{
    [SerializeField] SelectionFilter TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;
    [SerializeField] int maxRange;
    [SerializeField] bool selectAtRandom;
    [ShowIf("selectAtRandom")]
    [SerializeField] bool randomOnlySelectableTiles;

    public override string Description => TargetFilter.Equals(SelectionFilters.Self) ? null : $"Select {TargetFilter}";

    public override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        var patterns = ((ITilePatternBuilder)(maxRange == 0 ? new GlobalPatternBuilder() : new RangePatternBuilder { MinRange = 0, MaxRange = maxRange }))
            .Patterns(ctx.Source.Map, ctx.SourceTile, TargetFilter);
        if (selectAtRandom)
        {
            if (randomOnlySelectableTiles)
                patterns = patterns.Where(x => x.Selectable);
            return patterns.Any() ? new ITilePattern[] { patterns.ToArray()[Random.Range(0, patterns.Count())]
} : new ITilePattern[0];
        }
        else
            return patterns;
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();
        yield return null;
    }
}