using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Action")]
public class ActionCard : Card
{
    [SerializeField] Sprite icon;
    [SerializeField] new string name;
    [Range(1, 100)]
    [SerializeField] int speed;

    [Range(0, 10)]
    [SerializeField] int cost;

    [Required]
    public CardClass RequiredClass;

    public bool Once;
    public bool Temporary;


    public int Cost => cost;
    public override Sprite Icon => icon;

    public override string Name => name;

    public override int Speed => speed;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => Once || Temporary ? CardState.Destroyed : CardState.Played;
}
