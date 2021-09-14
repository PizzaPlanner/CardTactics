using System;
using System.Collections;
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

    public override IEnumerator Play(GameSettings settings, GameSettings.PlayerInfo player, Action onComplete)
    {
        yield return PlaySkills(settings, player);
        State = CardState.Active;
        if (Skills[0] is not PlaceSkill place || place.PlayProgress != Skill.Progress.Queued)
            State = CardState.Destroyed;

        if (State == CardState.Active && Passive != null)
            player.Deck.ExtendWith(Passive, this, Card.CardState.Active);

        onComplete?.Invoke();
    }
}
