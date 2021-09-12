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
}
