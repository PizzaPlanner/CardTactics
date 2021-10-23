using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    [SerializeField] Deck deck;

    [NonSerialized]
    [ShowInInspector]
    [ReadOnly]
    public Deck Deck;
    public int Team;
    public bool IsAI;
    public Color Color;

    [ShowIf("IsAI")]
    public bool RegisterLater;

    public LooseCondition LooseCondition;
    public Trigger OnLooseTrigger;

    internal void AIDecide(Card card, Skill skill, Tile[] validTiles)
    {
        if (validTiles.Length == 1)
            card.SkillTarget = validTiles[0];
        else
            card.SkillTarget = validTiles[UnityEngine.Random.RandomRange(0, validTiles.Length)];
    }

    public void Initialize()
    {
        Deck = Deck.Create(deck);
    }
}