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

    public void Initialize(HumanPlayer humanPlayer)
    {
        if (humanPlayer != null && IsAI)
            Debug.LogError("Incompatible Player Slot assignment");
        if (humanPlayer == null && Slot == SlotType.PlayerRequired)
            Debug.LogError("Incompatible Player Slot assignment");

        if (bringYourOwnDeck)
            Deck = Deck.Create(humanPlayer.Deck);
        else if (Deck == null)
            Deck = Deck.Create(deck);
    }
}