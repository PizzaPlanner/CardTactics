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
    [SerializeField] PlayerInfo PlayerTemplate;
    [HideIf("CopyPerSpawn")]
    [SerializeField] PlayerInfo[] Players;

    public Color Color;
    [SerializeReference] public List<TeamCondition> Conditions;

    public IEnumerable<PlayerInfo> RegisteredPlayers => registeredPlayers;
    List<PlayerInfo> registeredPlayers;

    public int MinPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired);
    public int MaxPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired || x.Slot == PlayerInfo.SlotType.PlayerOptional);

    public void Init(Board board, Map map)
    {
        if (CopyPerSpawn || Players == null) return;

        foreach (var player in Players)
        {
            if (!player.Initialize(Lobby.Current, this, map))
                continue;

            if (registeredPlayers == null)
                registeredPlayers = new List<PlayerInfo>();
            registeredPlayers.Add(player);
            var placeSkill = new PlaceSkill { reference = player.Deck.Commander };
            var skillInfo = new SkillInfo { Skill = placeSkill };
            board.Playables.InsertFirst(new SkillContext(skillInfo, player));
        }
    }

    internal Unit SpawnNew(Map map)
    {
        if (CopyPerSpawn && PlayerTemplate != null)
        {
            var player = PlayerInfo.Instantiate(PlayerTemplate);
            if (!player.Initialize(Lobby.Current, this, map))
                return null;

            if (registeredPlayers == null)
                registeredPlayers = new List<PlayerInfo>();
            registeredPlayers.Add(player);

            player.Deck.Commander.Player = player;

            return player.Deck.Commander;
        }
        return null;
    }
}