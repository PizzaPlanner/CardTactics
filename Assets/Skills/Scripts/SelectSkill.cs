using System.Collections;
using UnityEngine;

[System.Serializable]
public class SelectSkill : Skill, ICancelable
{
    [SerializeField] SelectionFilter TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;

    public override string Description => TargetFilter.Equals(SelectionFilters.Self) ? null : $"Select {TargetFilter}";

    protected override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        return map.ShowPlacementTiles(TargetFilter, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return null;
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
    }
}