using UnityEngine;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public override int Speed => 0;
    public Unit UnitReference;

    public Capacity[] Capacities;
    public int ClassesPerTurn;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => CardState.Active;
}
