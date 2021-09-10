using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Select")]
public class SelectSkill : Skill
{
    public TargetType TargetFilter;

    protected override bool OnActivate(int player, Card card)
    {
        return FindObjectOfType<Board>().ShowPlacementTiles(TargetFilter) > 0;
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