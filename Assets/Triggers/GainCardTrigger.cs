using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    [SerializeField] GainCardSkill Skill;

    [HideIf("GiveToTriggerer")]
    [SerializeField] int Player;
    [SerializeField] bool GiveToTriggerer;

    public override IEnumerator Execute(Board board, Map map, Unit unit, PlayerInfo player)
    {
        if (!GiveToTriggerer && (Player < 0 || Player >= map.Players.Count()))
        {
            Debug.LogWarning("Invalid Target for GainCardTrigger. Skipped.");
            yield break;
        }

        if (!GiveToTriggerer)
            player = map.Players.ToArray()[Player];
        var ctx = new SkillContext(new SkillInfo { Skill = Skill }, player, map.TileOf(unit));
        ctx.Play();
    }
}