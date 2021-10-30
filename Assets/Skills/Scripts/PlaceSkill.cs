using System;
using System.Collections;
using System.Collections.Generic;
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

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        switch (Type)
        {
            case PlaceType.Spawn:
                if (card is not UnitCard unitCard) return new Tile[0];
                this.card = unitCard;
                this.player = player;
                return map.ShowSpawnTiles(SelectionFilters.Empty, player);
            case PlaceType.Summon:
                var unit = Summon?.UnitReference ?? (SourceTile?.Unit);
                this.card = Summon ?? SourceTile?.Unit?.Card;
                if (unit == null || this.card == null)
                    return new Tile[0];
                this.player = player;
                return map.ShowRange(SourceTile, 1, maxRange, SelectionFilters.Empty, player);
        }
        return new Tile[0];
    }

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        if (Summon != null)
            list.Add(Summon);
        return list;
    }

    protected override IEnumerator OnPreview()
    {
        var unit = GameObject.Instantiate(card.UnitReference);
        unit.Card = card;
        unit.Player = player;
        if (Type == PlaceType.Summon && Summon == null)
            unit.Type = UnitFilter.Illusion;
        unit.gameObject.SetActive(false);
        var facing = TargetTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        TargetTile.Place(unit, facing);
        yield return null;
    }

    protected override void OnUse()
    {
    }

    public void OnCancel()
    {
        GameObject.Destroy(TargetTile.Unit.gameObject);
        TargetTile.Unit = null;
    }
}