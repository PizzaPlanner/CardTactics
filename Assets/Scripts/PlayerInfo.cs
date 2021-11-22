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
    public Deck Deck;

    [NonSerialized]
    public Team Team;
    public bool IsAI => Slot == SlotType.AI;
    public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Team.Color)}>{Deck.Commander.Name}</color>";

    public SlotType Slot => slot;

    [ShowInInspector]
    public bool IsAlive => Team != null && Deck.Commander != null;

    [NonSerialized] public bool IsLocal;

    [SerializeField] SlotType slot;

    public Trigger OnDead;

    internal void AIDecide(Card card, Skill skill, IEnumerable<ITilePattern> validPatterns)
    {
        card.SkillTarget = validPatterns.ToArray()[UnityEngine.Random.Range(0, validPatterns.Count())];
    }

    public bool Initialize(Lobby lobby, Team team)
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
            Deck = Deck.Create(lobby.DeckOf(this));
        else if (Deck == null)
            Deck = Deck.Create(deck);

        Team = team;
        return true;
    }
}