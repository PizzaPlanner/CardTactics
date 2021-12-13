using UnityEngine;

public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public Unit UnitReference;

    public override CardState StateOnPlayed => CardState.Destroyed;
}
