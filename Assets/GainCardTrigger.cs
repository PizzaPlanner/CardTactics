using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Use Skill")]
public class GainCardTrigger : Trigger
{
    public GainCardSkill Skill;

    public override void Execute(GameSettings settings, Tile tile)
    {
        Skill.Player = tile.Unit.Player;
        Skill.OnUse();
    }
}