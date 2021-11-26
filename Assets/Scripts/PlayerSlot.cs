using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[System.Serializable]
public class PlayerSlot : NetworkBehaviour
{
    public enum SlotType
    {
        PlayerRequired,
        PlayerOptional,
        AI
    }

    [HideIf("Deck")]
    [SerializeField] Deck deck;

    public Unit Unit { get; private set; }
    public Team Team { get; private set; }
    public bool IsAI => Slot == SlotType.AI;

    public SlotType Slot => slot;

    public bool IsAlive => Team != null && Unit != null && !Unit.IsDead;

    public NetworkIdentity Identity { get; private set; }

    [SerializeField] SlotType slot;

    public Trigger OnDeadSelf, OnDeadKiller;

    [Server]
    public bool Initialize(Team team, Map map)
    {
        var allPlayers = FindObjectsOfType<PlayerController>().ToList();
        allPlayers.Sort((x, y) => x.ControlledUnits.Count.CompareTo(y.ControlledUnits.Count));
        var player = Slot == SlotType.AI ? null : allPlayers.FirstOrDefault();
        if (Slot != SlotType.AI && player == null)
        {
            Debug.LogError("Incompatible Player Slot assignment");
            return false;
        }

        if (deck == null && (player == null || player.Deck == null)) return false;

        if (deck == null)
            Unit = player.Deck.Create(map);
        else
            Unit = deck.Create(map);
        Unit.Player = this;
        Team = team;
        if (player != null)
        {
            player.ControlledUnits.Add(Unit);
            Identity = player.GetComponent<NetworkIdentity>();
        }
        return true;
    }
}