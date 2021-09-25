using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    public TargetType TargetFilter;

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