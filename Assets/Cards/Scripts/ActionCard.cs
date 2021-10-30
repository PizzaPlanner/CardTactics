using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct PlayCost
{
    [HorizontalGroup(0.2f)]
    [HideLabel]
    public int Cost;
    [HorizontalGroup(0.8f)]
    [HideLabel]
    public CardClass Class;
}

[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [SerializeField] Sprite icon;
    [SerializeField] new string name;
    [SerializeField] PlayCost[] cost;

    public bool Once;
    public bool Temporary;


    public PlayCost[] Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => Once || Temporary ? CardState.Destroyed : CardState.Unplayed;

    public override void OnPlanningStart(Map map, PlayerInfo player)
    {
        if (State == CardState.Active && Once) Destroy();
        base.OnPlanningStart(map, player);
    }

    public override void OnPlanningEnd(Map map, PlayerInfo player)
    {
        if (Temporary && State == CardState.Unplayed) Destroy();
        base.OnPlanningEnd(map, player);
    }
}
