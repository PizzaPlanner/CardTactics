using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    [SerializeField] TargetType TargetFilter;
    [SerializeField] bool confirmOnePossibleTile;

    public override string Description => TargetFilter == TargetType.Ally ? null : $"Select {TargetFilter}";

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

    protected override void OnCancel()
    {
    }
}