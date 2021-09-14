using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    public TargetType TargetFilter;

    protected override Tile[] OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        return Settings.Map.ShowPlacementTiles(TargetFilter, player);
    }

    protected override void OnPreview()
    {
    }

    public override void OnUse()
    {
    }

    protected override void OnCancel()
    {
    }
}