using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    public TargetType TargetFilter;

    protected override bool OnActivate(int player, Card card, Tile selection)
    {
        FindObjectOfType<Board>().ShowPlacementTiles(TargetFilter);
        return true;
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