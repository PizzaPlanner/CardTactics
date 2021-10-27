using UnityEngine;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    [SerializeField] int speed;
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public override int Speed => speed;
    public Unit UnitReference;

    public PlayCost[] Capacities;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => CardState.Active;
}
