using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Place")]
public class PlaceSkill : Skill
{
    UnitCard card;
    GameSettings.PlayerInfo player;

    public bool Success { get; private set; }

    protected override bool OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        if (card is not UnitCard unitCard) return false;
        this.card = unitCard;
        this.player = player;
        return Settings.Map.ShowSpawnTiles(TargetType.Empty, Settings, player) > 0;
    }

    protected override void OnPreview()
    {
        var unit = GameObject.Instantiate(card.UnitReference);
        unit.Card = card;
        unit.Player = player;
        unit.gameObject.SetActive(false);
        TargetTile.Place(unit, 0);
    }

    public override void OnUse()
    {
        Success = TargetTile.Unit != null;
    }

    protected override void OnCancel()
    {
        GameObject.Destroy(TargetTile.Unit);
        TargetTile.Unit = null;
    }
}