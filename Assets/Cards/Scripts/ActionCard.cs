using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    public enum UseType
    {
        Default,
        Once,
        Temporary
    }


    [BoxGroup("Action", false, false, -1)]
    [HorizontalGroup("Action/Base", Width = 70)]
    [HideLabel]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite icon;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HideLabel]
    [SerializeField] new string name;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [EnumToggleButtons]
    [HideLabel]
    [SerializeField] UseType Type;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HideLabel]
    [SerializeField] ResourceInfo cost;


    public ResourceInfo Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    protected override void OnPlay(Board board, Map map, Unit unit, PlayerInfo player)
    {
        if (unit != null)
            unit.Resources.Spend(Cost);
    }

    protected override CardState StateOnPlayed => Type == UseType.Once || Type == UseType.Temporary ? CardState.Destroyed : CardState.Played;

    public override void OnRoundEnd(Map map, Unit unit)
    {
        if (Type == UseType.Temporary && State == CardState.Unplayed) Destroy();
        if (State == CardState.Active && Type == UseType.Once) Destroy();
        base.OnRoundEnd(map, unit);
    }

    public override string GetDescription()
    {
        var str = base.GetDescription();
        if (str.Length > 0) str += "\n";
        if (Type == UseType.Temporary)
            str += "Transient";
        else if (Type == UseType.Once)
            str += "Exhaust";
        return str;
    }
}
