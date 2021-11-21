using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlaceSkill : Skill
{
    [SerializeField] int maxRange;

    UnitCard card;
    PlayerInfo player;


    public override string Description => null;

    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        if (card is not UnitCard unitCard) return new ITilePattern[0];
        this.card = unitCard;
        this.player = player;
        return new SpawnPatternBuilder().Patterns(map, SourceTile, player, SelectionFilters.Empty);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        OutTile = targetPattern.Tiles.First();

        var unit = card.UnitReference;
        unit.Card = card;
        unit.Player = player;
        var facing = OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        OutTile.Place(unit, facing);
        yield return null;
    }

    protected override void OnUse()
    {
    }
}