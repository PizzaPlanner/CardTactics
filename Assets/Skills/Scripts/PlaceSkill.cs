using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Place")]
public class PlaceSkill : Skill
{
    UnitCard card;
    PlayerInfo player;

    protected override Tile[] OnActivate(GameSettings settings, PlayerInfo player, Card card)
    {
        if (card is not UnitCard unitCard) return new Tile[0];
        this.card = unitCard;
        this.player = player;
        return settings.Map.ShowSpawnTiles(TargetType.Empty, settings, player);
    }

    protected override IEnumerator OnPreview()
    {
        var unit = GameObject.Instantiate(card.UnitReference);
        unit.Card = card;
        unit.Player = player;
        unit.gameObject.SetActive(false);
        TargetTile.Place(unit, 0);
        yield return null;
    }

    protected override void OnUse()
    {
    }

    protected override void OnCancel()
    {
        GameObject.Destroy(TargetTile.Unit.gameObject);
        TargetTile.Unit = null;
    }
}