using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public enum SlotType
    {
        PlayerRequired,
        PlayerOptional,
        AI
    }

    public enum PlayerState
    {
        Hidden,
        Active,
        Dead
    }

    [HideIf("IsAI")]
    [SerializeField] bool bringYourOwnDeck;

    [HideIf("bringYourOwnDeck")]
    [HideIf("Deck")]
    [SerializeField] Deck deck;

    [ShowInInspector]
    [ReadOnly]
    [ShowIf("Deck")]
    [NonSerialized]
    public Deck Deck;
    public int Team;
    public bool IsAI => Slot == SlotType.AI;
    public Color Color;

    public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Color)}>{Deck.Commander.Name}</color>";

    [ShowInInspector]
    [NonSerialized]
    public PlayerState State;

    public SlotType Slot => slot;
    [SerializeField] SlotType slot;

    public LooseCondition LooseCondition;
    public Trigger OnLooseTrigger;

    internal void AIDecide(Card card, Skill skill, Tile[] validTiles)
    {
        if (validTiles.Length == 1)
            card.SkillTarget = validTiles[0];
        else
            card.SkillTarget = validTiles[UnityEngine.Random.RandomRange(0, validTiles.Length)];
    }

    public bool Initialize(Lobby lobby)
    {
        if (Slot == SlotType.PlayerRequired && !lobby.AssignPlayer(this))
        {
            Debug.LogError("Incompatible Player Slot assignment");
            return false;
        }
        else if (Slot == SlotType.PlayerOptional)
            if (!lobby.AssignPlayer(this))
                return false;

        if (bringYourOwnDeck && lobby.DeckOf(this) == null) return false;

        if (bringYourOwnDeck)
            Deck = Deck.Create(lobby.DeckOf(this));
        else if (Deck == null)
            Deck = Deck.Create(deck);

        State = PlayerState.Hidden;
        return true;
    }
}