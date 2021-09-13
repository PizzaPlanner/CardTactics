using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Card/Passive")]
public class PassiveCard : Card
{
    public override Sprite Icon => Passive.Icon;
    public override string Name => Passive.Name;
    public override int Speed => 0;
    public Passive Passive;
    public override int Cost => 0;

    protected override void OnInit()
    {
        Passive = Passive.Instantiate(Passive);
        Passive.Source = this;
    }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Active;
        Passive.OnActivate(settings, player);
    }
}
