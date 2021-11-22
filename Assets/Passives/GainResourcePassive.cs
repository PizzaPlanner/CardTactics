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

    public override string Description => $"{(Amount > 0 ? "Gain" : "Lose")} {Mathf.Abs(Amount)} {Class.Name}{(Type == GainType.DuringTurn ? " for your Turn" : Type == GainType.GainOnTurn ? " every Turn" : "")}";

    protected override void OnRemoved(Unit unit)
    {
        if (!gained) return;
        if (Type == GainType.ForDuration || Type == GainType.DuringTurn)
        {
            if (Amount < 0)
                unit.Resources.Add(Class, -Amount);
            else
                unit.Resources.Remove(Class, Amount);
            gained = false;
        }
    }

    bool gained;
    public override void OnTurnStart(Unit unit)
    {
        if (Type == GainType.GainOnTurn)
        {
            if (Amount < 0)
                unit.Resources.Remove(Class, -Amount);
            else
                unit.Resources.Add(Class, Amount);
        }
        else if (!gained && (Type == GainType.ForDuration || Type == GainType.DuringTurn))
        {
            if (Amount < 0)
                unit.Resources.Remove(Class, -Amount);
            else
                unit.Resources.Add(Class, Amount);
            gained = true;
        }
    }

    public override void OnTurnEnd(Unit unit)
    {
        if (!gained) return;
        if (Type == GainType.DuringTurn)
        {
            if (Amount < 0)
                unit.Resources.Add(Class, -Amount);
            else
                unit.Resources.Remove(Class, Amount);
            gained = false;
        }
    }
}