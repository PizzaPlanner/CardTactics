using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Attack")]
public class AttackSkill : Skill
{
    protected override bool OnActivate(int player, Card card, Tile selection)
    {
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