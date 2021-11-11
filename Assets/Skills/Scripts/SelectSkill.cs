using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SelectSkill : Skill, ICancelable
{
    [SerializeField] SelectionFilter TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;

    public override string Description => TargetFilter.Equals(SelectionFilters.Self) ? null : $"Select {TargetFilter}";

    protected override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        return new GlobalPatternBuilder().Patterns(map, SourceTile, player, TargetFilter);
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