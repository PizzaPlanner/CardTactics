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
        var map = Map.Instance;
        var ctx = new SkillPlanContext(new SkillInfo { Skill = Skill, SourceTileInput = SkillChain.A }, unit, map.TileOf(unit));
        yield return ctx.PlayOnServer();
    }
}