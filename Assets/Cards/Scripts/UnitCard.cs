using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public override int Speed => 0;
    public Unit UnitReference;
    public override int Cost => 0;
    public PassiveCard Passive;

    public Deck.Capacity[] Capacities;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => CardState.Active;
    protected override CardState StateOnSkipped => CardState.Destroyed;

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        if (Passive != null)
            list.Add(Passive);
        return list;
    }

    public override IEnumerator Play(GameSettings settings, GameSettings.PlayerInfo player, bool unused = false, Action onComplete = null)
    {
        yield return PlaySkills(settings, player, true);
        State = CardState.Active;
        if (Skills[0] is not PlaceSkill place || place.PlayProgress != Skill.Progress.Queued)
            State = CardState.Destroyed;

        if (State == CardState.Active && Passive != null)
            player.Deck.ExtendWith(Passive, this, Card.CardState.Active);

        onComplete?.Invoke();
    }
}
