using System;
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

    [HideLabel]
    [SerializeField] new string name;
    [HideLabel]
    [SerializeField] Sprite icon;

    [HideLabel]
    [SerializeField] ResourceInfo cost;

    public bool Once;
    public bool Temporary;


    public ResourceInfo Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    protected override void OnPlay(Board board, Map map, Unit unit, PlayerInfo player)
    {
        if (unit != null)
            unit.Resources.Spend(Cost);
    }

    protected override CardState StateOnPlayed => Once || Temporary ? CardState.Destroyed : CardState.Played;

    public override void OnRoundEnd(Map map, PlayerInfo player)
    {
        if (Temporary && State == CardState.Unplayed) Destroy();
        if (State == CardState.Active && Once) Destroy();
        base.OnRoundEnd(map, player);
    }
}
