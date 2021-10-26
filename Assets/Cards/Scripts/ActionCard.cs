using System;
using Sirenix.OdinInspector;
using Unity.Burst.CompilerServices;
using UnityEngine;

[System.Serializable]
public struct PlayCost
{
    public CardClass Class;
    public int Cost;
}

[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [SerializeField] Sprite icon;
    [SerializeField] new string name;
    [Range(1, 100)]
    [SerializeField] int speed;
    [SerializeField] PlayCost[] cost;

    public bool Once;
    public bool Temporary;


    public PlayCost[] Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    public override int Speed => speed;

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
