using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerSlot : MonoBehaviour
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

    [ShowInInspector]
    public bool IsAlive => Team != null && Unit != null && !Unit.IsDead;

    [NonSerialized] public bool IsLocal;

    [SerializeField] SlotType slot;

    public Trigger OnDeadSelf, OnDeadKiller;

    public bool Initialize(Lobby lobby, Team team, Map map)
    {
        if (Slot == SlotType.PlayerRequired && !lobby.AssignPlayer(this))
        {
            Debug.LogError("Incompatible Player Slot assignment");
            return false;
        }
        else if (Slot == SlotType.PlayerOptional)
            if (!lobby.AssignPlayer(this))
                return false;

        if (deck == null && lobby.DeckOf(this) == null) return false;

        if (deck == null)
            Unit = lobby.DeckOf(this).Create(map);
        else
            Unit = deck.Create(map);
        Unit.Player = this;
        Team = team;
        return true;
    }
}