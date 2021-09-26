using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public Deck Deck;
    public int Team;
    public bool IsAI;
    public Color Color;

    [ShowIf("IsAI")]
    public bool RegisterLater;

    public LooseCondition LooseCondition;
    public int OnLooseTrigger;

    internal void AIDecide(Card card, Skill skill, Tile[] validTiles)
    {
        card.SkillTarget = validTiles[UnityEngine.Random.RandomRange(0, validTiles.Length)];
    }
}