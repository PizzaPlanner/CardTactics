using UnityEngine;

[CreateAssetMenu(menuName = "Card/Commander")]
public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public Unit UnitReference;

    protected override CardState StateOnPlayed => CardState.Active;
}
