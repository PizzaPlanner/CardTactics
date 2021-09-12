using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class ActionCard : Card
{
    [SerializeField] Texture2D icon;
    [SerializeField] string name;
    [Range(1, 100)]
    [SerializeField] int speed;

    [Range(0, 10)]
    [SerializeField] int cost;


    public override int Cost => cost;
    public override bool Once => false;
    public override Texture2D Icon => icon;

    public override string Name => name;

    public override int Speed => speed;
    public override string Category => "Action";

    protected override void OnInit() { }

    public override void Play(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Played;
    }
}
