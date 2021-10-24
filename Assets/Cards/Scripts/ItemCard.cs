using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Item")]
public class ItemCard : Card
{
    [SerializeField] int speed;
    public int actions;
    [SerializeField] int chargeCost;


    [SerializeField] Sprite icon;
    [SerializeField] new string name;

    [Required]
    public CardClass ItemClass;

    public override Sprite Icon => icon;
    public override string Name => name;
    public override int Speed => speed;

    protected override void OnInit()
    {
        State = CardState.Active;
    }

    protected override CardState StateOnPlayed => CardState.Active;

    public override IEnumerator Play(Map map, PlayerInfo player)
    {
        yield return null;
    }
}
