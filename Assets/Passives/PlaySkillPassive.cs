using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class PlayCardPassive : Passive
{
    public enum PlayType
    {
        OnTurnStart,
        OnTurnEnd,
        OnRemoved
    }

    [HideLabel]
    [SerializeField] Card Card;
    [SerializeField] PlayType When;

    public override string Description => $"{(When == PlayType.OnTurnStart ? "At the Start of your Turn:\n" : When == PlayType.OnTurnEnd ? "At the end of your Turn:\n" : "")}{Card.GetDescription()}";

    void PlayCard(Unit unit)
    {
        var card = Card.Instantiate(Card);
        card.Init();
        Card.FindObjectOfType<Board>().Playables.InsertFirst(unit.Player, unit, card);
    }

    protected override void OnRemoved(Unit unit)
    {
        if (When != PlayType.OnRemoved) return;
        PlayCard(unit);
    }

    public override void OnTurnStart(Unit unit)
    {
        if (When != PlayType.OnTurnStart) return;
        PlayCard(unit);
    }

    public override void OnTurnEnd(Unit unit)
    {
        if (When != PlayType.OnTurnEnd) return;
        PlayCard(unit);
    }
}