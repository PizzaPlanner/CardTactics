using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Item")]
public class ItemCard : Card
{
    public int actions;
    [SerializeField] int chargeCost;


    [SerializeField] Sprite icon;
    [SerializeField] new string name;

    [Required]
    public CardClass ItemClass;

    public override Sprite Icon => icon;
    public override string Name => name;

    public override CardState StateOnPlayed => CardState.Active;
}
