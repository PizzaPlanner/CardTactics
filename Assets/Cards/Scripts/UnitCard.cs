using UnityEngine;
using UnityEngine.UI;

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

    public override void Skip(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Destroyed;
    }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Destroyed;
        foreach (var skill in Skills)
        {
            if (skill is PlaceSkill place && place.Success)
                State = CardState.Active;
        }

        if (State == CardState.Active && Passive != null)
            player.Deck.ExtendWith(Passive, this, Card.CardState.Active);
    }
}
