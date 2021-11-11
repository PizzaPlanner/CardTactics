using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DashSkill : Skill
{
    public int MaxRange;
    public RangeFilter RangeType;

    public override string Description => $"Dash {MaxRange}";

    protected override bool AutoSelectOnOnePossibleTile => false;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        var filter = SelectionFilters.Empty;
        filter.Range = RangeType;
        if (SourceTile.Unit == null)
            return new ITilePattern[0];
        return new CrosshairPatternBuilder { Length = MaxRange, Offset = 1 }.Patterns(map, SourceTile, player, filter);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        var prevTile = SourceTile;
        foreach (var tile in targetPattern.Tiles)
            if (tile.Unit == null)
            {
                yield return prevTile.Unit.Move(prevTile, new EmptyPattern(), tile, RangeType);
                prevTile = tile;
            }
        OutTile = targetPattern.Tiles.Last(x => x.CurrentIndicator == TargetIndicator.Valid);
    }

    protected override void OnUse()
    {
    }
}