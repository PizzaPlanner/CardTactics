using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    [SerializeField] GainCardSkill Skill;

    public override IEnumerator Execute(Board board, Map map, Unit unit, PlayerInfo player)
    {
        var ctx = new SkillContext(new SkillInfo { Skill = Skill, SourceTileInput = SkillChain.A }, player, map.TileOf(unit));
        yield return ctx.Play();
    }
}