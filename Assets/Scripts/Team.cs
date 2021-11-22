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
    public LooseCondition LooseCondition;
    public Trigger OnLooseTrigger;

    public IEnumerable<PlayerInfo> RegisteredPlayers => registeredPlayers;
    List<PlayerInfo> registeredPlayers;

    public int MinPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired);
    public int MaxPlayers => CopyPerSpawn || Players == null ? 0 : Players.Count(x => x.Slot == PlayerInfo.SlotType.PlayerRequired || x.Slot == PlayerInfo.SlotType.PlayerOptional);

    public void Init(Board board)
    {
        if (CopyPerSpawn || Players == null) return;

        foreach (var player in Players)
        {
            if (!player.Initialize(Lobby.Current, this))
                continue;

            if (registeredPlayers == null)
                registeredPlayers = new List<PlayerInfo>();
            registeredPlayers.Add(player);
            var placeCard = new UnitCard { UnitReference = player.Deck.Commander };
            placeCard.Init();
            board.Playables.InsertFirst(player, null, placeCard);
        }
    }

    internal Unit SpawnNew()
    {
        if (CopyPerSpawn && PlayerTemplate != null)
        {
            var player = PlayerInfo.Instantiate(PlayerTemplate);
            if (!player.Initialize(Lobby.Current, this))
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