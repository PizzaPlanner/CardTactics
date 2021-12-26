using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [BoxGroup("Action", false, false, -1)]
    [HorizontalGroup("Action/Base", Width = 70)]
    [VerticalGroup("Action/Base/Left")]
    [HideLabel]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite icon;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HorizontalGroup("Action/Base/Right/Top")]
    [BoxGroup("Action/Base/Right/Top/Name")]
    [HideLabel]
    [SerializeField] new string name;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HorizontalGroup("Action/Base/Right/Top", Width = 50)]
    [BoxGroup("Action/Base/Right/Top/Charges")]
    [HideLabel]
    [SerializeField] public int charges;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HorizontalGroup("Action/Base/Right/Top", Width = 100)]
    [BoxGroup("Action/Base/Right/Top/Final State")]
    [HideLabel]
    [SerializeField] CardState FinalState = CardState.Destroyed;
    public override CardState DestroyedOn => FinalState;

    [BoxGroup("Action")]
    [HorizontalGroup("Action/Base")]
    [VerticalGroup("Action/Base/Right")]
    [HorizontalGroup("Action/Base/Right/Sub")]
    [HideLabel]
    [SerializeField] ResourceInfo cost;


    public ResourceInfo Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    public override void OnPlay(CardContext context)
    {
        if (context.Unit != null)
            context.Unit.Resources.Spend(Cost);
    }

    protected override string GetDescription()
    {
        var str = base.GetDescription();
        if (str.Length > 0) str += "\n";
        return str;
    }
}
