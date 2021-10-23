using System.Collections;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    public GainCardSkill Skill;

    protected override IEnumerator Execute(Map map, Tile tile)
    {
        Skill.Player = tile?.Unit.Player ?? map.Players[0];
        yield return Skill.Preview(tile);
        Skill.Use();
    }
}