using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class HealSkill : Skill
{
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;

    public float Heal;

    Unit previewUnit;
    public override string Description => $"Heal {Heal}";
    protected override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        if ((Filter.Tile & TileFilter.Empty) > 0) return new ITilePattern[0];
        return new RangePatternBuilder { MinRange = MinRange, MaxRange = MaxRange }.Patterns(map, SourceTile, player, Filter);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        OutTile = targetPattern.Tiles.First();
        SourceTile.Unit.GetComponentInChildren<Animator>().SetTrigger("Cast");
        yield return new WaitForSeconds(0.5f);
        previewUnit = OutTile.Unit;
        previewUnit.Damage(-Heal, true);
    }

    protected override void OnUse()
    {
        previewUnit.Damage(-Heal, false);
    }
}