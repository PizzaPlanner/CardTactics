using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class UnitCard : Card
{
    public override Texture2D Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public override int Speed => 0;
    public Unit UnitReference;
    public override int Cost => 5;
    public override bool Once => true;
    public override string Category => "Unit";
    public Passive Passive;
    protected override void OnInit()
    {
        Passive = Passive.Instantiate(Passive);
        Passive.Source = this;
    }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Active;
        Passive?.OnActivate(settings, player);
    }
}
