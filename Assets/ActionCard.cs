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
}
