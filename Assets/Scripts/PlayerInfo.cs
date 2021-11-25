using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerInfo : MonoBehaviour
{
    public enum SlotType
    {
        PlayerRequired,
        PlayerOptional,
        AI
    }

    [HideIf("Deck")]
    [SerializeField] Deck deck;

    [ShowInInspector]
    [ReadOnly]
    [ShowIf("Deck")]
    [NonSerialized]
    public IngameDeck Deck;

    [NonSerialized]
    public Team Team;
    public bool IsAI => Slot == SlotType.AI;
    public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Team.Color)}>{Deck.Commander.Name}</color>";

    public SlotType Slot => slot;

    [ShowInInspector]
    public bool IsAlive => Team != null && Deck.Commander != null && !Deck.Commander.IsDead;

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
            Deck = new IngameDeck(lobby.DeckOf(this), map);
        else if (Deck == null)
            Deck = new IngameDeck(deck, map);

        Team = team;
        return true;
    }
}