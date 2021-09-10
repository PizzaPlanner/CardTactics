using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Place")]
public class PlaceSkill : Skill
{
    Unit unit;

    protected override bool OnActivate(int player, Card card)
    {
        if (card is not UnitCard) return false;
        unit = GameObject.Instantiate((card as UnitCard).UnitReference);
        unit.Player = player;
        unit.gameObject.SetActive(false);
        return FindObjectOfType<Board>().ShowPlacementTiles(TargetType.Empty) > 0;
    }

    protected override void OnPreview()
    {
        PreviewTile.Place(unit, 0);
    }

    public override void OnUse()
    {

    }

    protected override void OnCancel()
    {
        PreviewTile.Unit = null;
        GameObject.Destroy(unit.gameObject);
    }
}