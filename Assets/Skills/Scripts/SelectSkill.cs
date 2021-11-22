using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SelectSkill : Skill, ICancelable
{
    [SerializeField] SelectionFilter TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;
    [SerializeField] int maxRange;
    [SerializeField] bool selectAtRandom;
    [ShowIf("selectAtRandom")]
    [SerializeField] bool randomOnlySelectableTiles;

    public override string Description => TargetFilter.Equals(SelectionFilters.Self) ? null : $"Select {TargetFilter}";

    protected override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        var patterns = ((ITilePatternBuilder)(maxRange == 0 ? new GlobalPatternBuilder() : new RangePatternBuilder { MinRange = 0, MaxRange = maxRange })).Patterns(map, SourceTile, player, TargetFilter);
        if (selectAtRandom)
        {
            if (randomOnlySelectableTiles)
                patterns = patterns.Where(x => x.Selectable);
            return patterns.Any() ? new ITilePattern[] { patterns.ToArray()[Random.Range(0, patterns.Count())] } : new ITilePattern[0];
        }
        else
            return patterns;
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        OutTile = targetPattern.Tiles.First();
        yield return null;
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
    }
}