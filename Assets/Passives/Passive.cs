using System.Collections;
using System.Collections.Generic;

public abstract class Passive : Skill
{
    protected override bool AutoSelectOnOnePossibleTile => true;

    public abstract void OnRoundStart(Map map, PlayerInfo player);

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        return new[] { new SingleTilePattern { Target = SourceTile }.Build(map, SourceTile, player, new SelectionFilter { Range = RangeFilter.Any }) };
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        yield return null;
    }

    protected override void OnUse()
    {
    }
}