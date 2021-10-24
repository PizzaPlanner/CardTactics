using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class GainCardTrigger : Trigger
{
    [SerializeField] GainCardSkill Skill;

    [HideIf("GiveToTriggerer")]
    [SerializeField] int Player;
    [SerializeField] bool GiveToTriggerer;

    protected override IEnumerator Execute(Map map, PlayerInfo player)
    {
        if (!GiveToTriggerer && (Player < 0 || Player >= map.Players.Length))
        {
            Debug.LogWarning("Invalid Target for GainCardTrigger. Skipped.");
            yield break;
        }

        Skill.Player = GiveToTriggerer ? player : map.Players[Player];
        yield return GainCardSkill.ShowGetAnimation(Skill.CardPrefab);
        Skill.Use();
    }
}