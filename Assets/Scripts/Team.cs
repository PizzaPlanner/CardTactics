using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Team
{
    [SerializeField] bool CopyPerSpawn;
    [ShowIf("CopyPerSpawn")]
    [SerializeField] PlayerSlot PlayerTemplate;
    [HideIf("CopyPerSpawn")]
    [SerializeField] PlayerSlot[] Players;

    public Color Color;
    [SerializeReference] public List<TeamCondition> Conditions;

    public int MinPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerSlot.SlotType.PlayerRequired);
    public int MaxPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerSlot.SlotType.PlayerRequired || x.Slot == PlayerSlot.SlotType.PlayerOptional);

    public void CreatePlayables(PlayableQueue queue, Map map)
    {
        if (CopyPerSpawn || Players == null) return;

        foreach (var player in Players)
        {
            if (!player.GetUnit(this, map, out var unit))
                continue;

            if (queue == null) return;

            var placeSkill = new PlaceSkill { reference = unit };
            var skillInfo = new SkillInfo { Skill = placeSkill };
            queue.InsertFirst(new SkillContext(skillInfo, unit, map));
        }
    }

    internal Unit SpawnNew(Map map)
    {
        if (CopyPerSpawn && PlayerTemplate != null)
        {
            if (!PlayerTemplate.GetUnit(this, map, out var unit))
                return null;

            return unit;
        }
        return null;
    }
}