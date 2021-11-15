using UnityEngine;

[System.Serializable]
public class GainResourcePassive : Passive
{
    [SerializeField] CardClass Class;
    [SerializeField] int Amount;

    public override string Description => $"This unit can use Cards that require {Class.Name}";
    protected override TargetType Target => TargetType.Unit;

    public override void OnActivation()
    {

        TargetUnit.Resources.Add(Class, Amount);
    }

    public override void OnPassiveEnd()
    {
        TargetUnit.Resources.Remove(Class, Amount);
    }
}