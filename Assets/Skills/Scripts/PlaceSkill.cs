using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Place")]
public class PlaceSkill : Skill
{
    Unit unit;

    protected override bool OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        if (card is not UnitCard unitCard) return false;
        unit = GameObject.Instantiate(unitCard.UnitReference);
        unit.Card = unitCard;
        unit.Player = player;
        unit.gameObject.SetActive(false);
        return Settings.Map.ShowPlacementTiles(TargetType.Empty, player) > 0;
    }

    protected override void OnPreview()
    {
        TargetTile.Place(unit, 0);
    }

    public override void OnUse()
    {

    }

    protected override void OnCancel()
    {
        TargetTile.Unit = null;
        GameObject.Destroy(unit.gameObject);
    }
}