using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

public class IngameDeck
{
    public List<CardContext> Contexts;

    public void OnRoundStart()
    {
        foreach (var card in Contexts)
            card.Target.OnRoundStart(card);
    }

    public void OnRoundEnd()
    {
        foreach (var card in Contexts)
            card.Target.OnRoundEnd(card);
    }

    public void OnTurnStart()
    {
        foreach (var card in Contexts)
            card.Target.OnTurnStart(card);
    }

    public void OnTurnEnd()
    {
        foreach (var card in Contexts)
            card.Target.OnTurnEnd(card);
    }

    public IngameDeck(Deck prefab, Map map)
    {
        Contexts = new List<CardContext>();

        Commander = Card.Instantiate(prefab.commanderPrefab);
        Commander.gameObject.SetActive(false);
        foreach (var card in prefab.Cards)
            Contexts.Add(new CardContext(card, map, Commander, CardState.Unplayed));
    }

    [NonSerialized] public Unit Commander;

    public IEnumerable<CardContext> All => Contexts;
    public IEnumerable<CardContext> Unplayed => Contexts.Where(x => x.State == CardState.Unplayed);
    public IEnumerable<CardContext> Playable => Unplayed.Where(x => x.Target is ActionCard acard && x.Unit.Resources.CanAfford(acard.Cost));


#if UNITY_EDITOR
    [BoxGroup("Editor")]
    [ShowInInspector] Card editorAddCardPrefab;

    [BoxGroup("Editor")]
    [Button("Add Queued Card")]
    void EditorAddCard()
    {

    }
#endif
}

[CreateAssetMenu]
public class Deck : ScriptableObject
{
    public Unit commanderPrefab;
    public Card[] Cards;
}