using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public override int Speed => 0;
    public Unit UnitReference;
    public override int Cost => 5;
    public PassiveCard Passive;

    public Deck.Capacity[] Capacities;

    protected override void OnInit()
    {
        if (Passive != null)
        {
            Passive = PassiveCard.Instantiate(Passive);
            Passive.Init(this);
        }
    }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Active;
        Passive?.Play(settings, player);
    }
}
