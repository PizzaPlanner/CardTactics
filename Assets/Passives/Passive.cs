using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Passive : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;

    [NonSerialized] public Card Source;

    public virtual List<Card> GetRelatedCards()
    {
        var list = new List<Card>();
        if (Source != null)
            list.Add(Source);
        return list;
    }

    public abstract void OnRoundStart(Map map, PlayerInfo player);
}