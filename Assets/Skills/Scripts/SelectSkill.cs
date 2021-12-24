using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SelectSkill : ISkill
{
    [SerializeField] SelectionFilter TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;
    [SerializeField] int maxRange;
    [SerializeField] bool selectAtRandom;
    [ShowIf("selectAtRandom")]
    [SerializeField] bool randomOnlySelectableTiles;

    public string Description => TargetFilter.Equals(SelectionFilters.Self) ? null : $"Select {TargetFilter}";

    public bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        var patterns = ((ITilePatternBuilder)(maxRange == 0 ? new GlobalPatternBuilder() : new RangePatternBuilder { MinRange = 0, MaxRange = maxRange }))
            .Patterns(Map.Instance, ctx.SourceTile, TargetFilter);
        if (selectAtRandom)
        {
            if (randomOnlySelectableTiles)
                patterns = patterns.Where(x => x.Selectable);
            return patterns.Any() ? new ITilePattern[] { patterns.ToArray()[Random.Range(0, patterns.Count())] } : new ITilePattern[0];
        }
        else
            return patterns;
    }

    public void Serialize(NetworkWriter writer) { }
    public ISkill Deserialize(NetworkReader reader) { return this; }
}