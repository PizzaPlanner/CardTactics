using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Place")]
public class PlaceSkill : Skill
{
    UnitCard card;
    GameSettings.PlayerInfo player;

    public bool Success { get; private set; }

    protected override Tile[] OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        if (card is not UnitCard unitCard) return new Tile[0];
        this.card = unitCard;
        this.player = player;
        return Settings.Map.ShowSpawnTiles(TargetType.Empty, Settings, player);
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

    public override void OnUse()
    {
        Success = TargetTile.Unit != null;
    }

    protected override void OnCancel()
    {
        GameObject.Destroy(TargetTile.Unit.gameObject);
        TargetTile.Unit = null;
    }
}