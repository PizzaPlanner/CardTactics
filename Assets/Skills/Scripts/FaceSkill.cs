using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class FaceSkill : Skill
{
    public SelectionFilter Filter;
    public int minRange;
    public int maxRange;

    Facing originalFacing;
    Facing targetFacing;

    public override string Description => $"Look at";
    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        var unit = SourceTile?.Unit;
        if (unit == null)
            return new Tile[0];
        originalFacing = SourceTile.Facing;
        return map.ShowRange(SourceTile, minRange, maxRange, Filter, player);
    }

    protected override IEnumerator OnPreview()
    {
        targetFacing = Tile.GetFacing(SourceTile, TargetTile);
        SourceTile.Face(targetFacing);
        yield return null;
    }

    protected override void OnUse()
    {
        SourceTile.Face(targetFacing);
    }
}