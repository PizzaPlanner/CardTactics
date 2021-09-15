using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    public TargetType TargetFilter;

    protected override Tile[] OnActivate(GameSettings settings, GameSettings.PlayerInfo player, Card card)
    {
        return settings.Map.ShowPlacementTiles(TargetFilter, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return null;
    }

    public override void OnUse()
    {
    }

    protected override void OnCancel()
    {
    }
}