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
        Card.FindObjectOfType<Board>().Playables.InsertFirst(new CardContext(Card, Map.FindObjectOfType<Map>(), unit, CardState.Unplayed));
    }

    protected override void OnRemoved(CardContext ctx)
    {
        if (When != PlayType.OnRemoved) return;
        PlayCard(ctx.Unit);
    }

    public override void OnTurnStart(CardContext ctx)
    {
        if (When != PlayType.OnTurnStart) return;
        PlayCard(ctx.Unit);
    }

    public override void OnTurnEnd(CardContext ctx)
    {
        if (When != PlayType.OnTurnEnd) return;
        PlayCard(ctx.Unit);
    }
}