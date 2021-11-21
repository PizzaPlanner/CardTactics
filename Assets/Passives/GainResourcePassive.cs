using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class GainResourcePassive : Passive
{
    public enum GainType
    {
        ForDuration,
        DuringTurn,
        GainOnTurn
    }

    [HideLabel]
    [SerializeField] CardClass Class;
    [SerializeField] int Amount;

    [SerializeField] GainType Type;

    public override string Description => Type == GainType.ForDuration ? $"Gain {Amount} {Class.Name}" : Type == GainType.DuringTurn ? $"Gain {Amount} {Class.Name} during your Turn" : $"Gain {Amount} {Class.Name} at the start of each Turn";

    protected override void OnRemoved(Unit unit)
    {
        if (Type == GainType.ForDuration)
            unit.Resources.Remove(Class, Amount);
    }

    bool gained;
    public override void OnTurnStart(Unit unit)
    {
        Debug.Log("Add something for " + unit.gameObject.name);
        if (Type == GainType.DuringTurn || Type == GainType.GainOnTurn)
            unit.Resources.Add(Class, Amount);
        else if (Type == GainType.ForDuration && !gained)
        {
            unit.Resources.Add(Class, Amount);
            gained = true;
        }
    }

    public override void OnTurnEnd(Unit unit)
    {
        if (Type == GainType.DuringTurn)
            unit.Resources.Remove(Class, Amount);
    }
}