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

    protected override CardState StateOnPlayed => Once || Temporary ? CardState.Destroyed : CardState.Played;

    public override void OnRoundStart(Map map, PlayerInfo player)
    {
        if (State == CardState.Unplayed && Temporary) Destroy();
        base.OnRoundStart(map, player);
        // This destroy after base call to trigger effects
        if (State == CardState.Active && Temporary) Destroy();
    }
}
