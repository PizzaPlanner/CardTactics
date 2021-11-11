using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlaceSkill : Skill, ICancelable
{
    public enum PlaceType
    {
        Spawn,
        Summon
    }
    [SerializeField] PlaceType Type;
    [SerializeField] UnitCard Summon;

    [SerializeField] int maxRange;

    UnitCard card;
    PlayerInfo player;


    public override string Description => Type == PlaceType.Spawn ? null : $"Summon {Summon?.UnitReference.Name ?? "Illusion" }";

    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        switch (Type)
        {
            case PlaceType.Spawn:
                if (card is not UnitCard unitCard) return new ITilePattern[0];
                this.card = unitCard;
                this.player = player;
                return new SpawnPatternBuilder().Patterns(map, SourceTile, player, SelectionFilters.Empty);
            case PlaceType.Summon:
                var unit = Summon?.UnitReference ?? (SourceTile?.Unit);
                this.card = Summon ?? SourceTile?.Unit?.Card;
                if (unit == null || this.card == null)
                    return new ITilePattern[0];
                this.player = player;
                return new RangePatternBuilder { MinRange = 1, MaxRange = maxRange }.Patterns(map, SourceTile, player, SelectionFilters.Empty);
        }
        return new ITilePattern[0];
    }

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        if (Summon != null)
            list.Add(Summon);
        return list;
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        OutTile = targetPattern.Tiles.First();
        var unit = GameObject.Instantiate(card.UnitReference);
        unit.Card = card;
        unit.Player = player;
        if (Type == PlaceType.Summon && Summon == null)
            unit.Type = UnitFilter.Illusion;
        unit.gameObject.SetActive(false);
        var facing = OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        OutTile.Place(unit, facing);
        yield return null;
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
        GameObject.Destroy(OutTile.Unit.gameObject);
        OutTile.Unit = null;
    }
}