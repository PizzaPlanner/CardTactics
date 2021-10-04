using System.Collections;

public abstract class Passive : Skill
{
    protected override bool AutoSelectOnOnePossibleTile => true;

    public abstract void OnRoundStart(Map map, PlayerInfo player);

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        return map.ShowRange(SourceTile, 0, 0, TargetType.Ally, RangeType.Any, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return null;
    }

    protected override void OnUse()
    {
    }
}