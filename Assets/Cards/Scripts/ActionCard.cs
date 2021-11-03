using System;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [SerializeField] Sprite icon;
    [SerializeField] new string name;
    [SerializeField] ResourceInfo cost;

    public bool Once;
    public bool Temporary;


    public ResourceInfo Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    protected override void OnInit() { }

    protected override void OnPlay(Board board, Map map, Unit unit, PlayerInfo player)
    {
        if (unit != null)
        {
            unit.Resources.EnergySpent += Cost.Energy;
            Debug.Log(unit.Resources.EnergySpent);
        }
    }

    protected override CardState StateOnPlayed => Once || Temporary ? CardState.Destroyed : CardState.Unplayed;

    public override void OnTurnStart(Map map, PlayerInfo player)
    {
        if (State == CardState.Active && Once) Destroy();
        base.OnTurnStart(map, player);
    }

    public override void OnTurnEnd(Map map, PlayerInfo player)
    {
        if (Temporary && State == CardState.Unplayed) Destroy();
        base.OnTurnEnd(map, player);
    }
}
