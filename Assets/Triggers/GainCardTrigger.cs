using System.Collections;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    public GainCardSkill Skill;

    public override IEnumerator Execute(GameSettings settings, Tile tile)
    {
        Skill.Player = tile.Unit.Player;
        yield return Skill.Preview(tile);
        Skill.Use();
    }
}