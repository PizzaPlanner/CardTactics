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

    protected override CardState StateOnPlayed => CardState.Active;
    protected override CardState StateOnSkipped => CardState.Skipped;
}
