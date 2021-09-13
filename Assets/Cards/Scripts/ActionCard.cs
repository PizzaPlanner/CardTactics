using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [SerializeField] Sprite icon;
    [SerializeField] new string name;
    [Range(1, 100)]
    [SerializeField] int speed;

    [Range(0, 10)]
    [SerializeField] int cost;

    public bool Once;
    public bool Temporary;


    public override int Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    public override int Speed => speed;

    protected override void OnInit() { }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = Once || Temporary ? CardState.Destroyed : CardState.Played;
    }

    public override void Skip(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = Temporary ? CardState.Destroyed : CardState.Played;
    }
}
