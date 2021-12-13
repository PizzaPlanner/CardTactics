using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    [SerializeField] GainCardSkill Skill;

    public override IEnumerator Execute(Unit unit)
    {
        var map = FindObjectOfType<Map>();
        var ctx = new SkillContext(new SkillInfo { Skill = Skill, SourceTileInput = SkillChain.A }, unit, map, map.TileOf(unit));
        yield return ctx.PlayOnServer();
    }
}