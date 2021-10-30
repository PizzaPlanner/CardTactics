using UnityEngine;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public Unit UnitReference;

    public PlayCost[] Capacities;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => CardState.Active;
}
