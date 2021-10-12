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

    public bool Once;

    public override Sprite Icon => icon;
    public override string Name => name;
    public override int Speed => 0;

    protected override void OnInit() { }

    protected override CardState StateOnPlayed => Once ? CardState.Destroyed : CardState.Played;

    public override IEnumerator Play(Map map, PlayerInfo player, bool autocompleteIfPossible = true, Action onComplete = null)
    {
        yield return null;
    }
}
