using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Triggers/Use Skill")]
public class GainCardTrigger : Trigger
{
    public GainCardSkill Skill;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        Skill.Player = tile.Unit.Player;
        yield return Skill.Preview(tile);
        Skill.OnUse();
    }
}