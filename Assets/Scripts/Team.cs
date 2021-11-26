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

    public IEnumerable<PlayerSlot> RegisteredPlayers => registeredPlayers;
    List<PlayerSlot> registeredPlayers;

    public int MinPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerSlot.SlotType.PlayerRequired);
    public int MaxPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerSlot.SlotType.PlayerRequired || x.Slot == PlayerSlot.SlotType.PlayerOptional);

    public void Init(Board board, Map map)
    {
        if (CopyPerSpawn || Players == null) return;

        foreach (var player in Players)
        {
            if (!player.Initialize(this, map))
                continue;

            if (registeredPlayers == null)
                registeredPlayers = new List<PlayerSlot>();
            registeredPlayers.Add(player);
            var placeSkill = new PlaceSkill { reference = player.Unit };
            var skillInfo = new SkillInfo { Skill = placeSkill };
            board.Playables.InsertFirst(new SkillContext(skillInfo, player.Unit));
        }
    }

    internal Unit SpawnNew(Map map)
    {
        if (CopyPerSpawn && PlayerTemplate != null)
        {
            var player = PlayerSlot.Instantiate(PlayerTemplate);
            if (!player.Initialize(this, map))
                return null;

            if (registeredPlayers == null)
                registeredPlayers = new List<PlayerSlot>();
            registeredPlayers.Add(player);

            return player.Unit;
        }
        return null;
    }
}