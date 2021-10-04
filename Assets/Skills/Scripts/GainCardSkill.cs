using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Gain Card")]
public class GainCardSkill : Skill
{
    [NonSerialized] public PlayerInfo Player;

    [SerializeField] Card NewCardPrefab;
    [SerializeField] Card.CardState NewCardInitialState;

    public Card CardPrefab => NewCardPrefab;

    public override string Description => $"Gain {CardPrefab.Name} ({NewCardInitialState})";

    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        Player = player;
        return map.ShowRange(SourceTile, 0, 0, TargetType.Ally, RangeType.Any, player);
    }

    protected override IEnumerator OnPreview()
    {
        yield return new WaitForEndOfFrame();
        CardTooltip.Show(NewCardPrefab);
        yield return new WaitForSeconds(2);
        CardTooltip.Hide();
    }

    protected override void OnUse()
    {
        Player.Deck.ExtendWith(NewCardPrefab, Source, NewCardInitialState);
    }

    protected override void OnCancel()
    {
    }

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.Add(CardPrefab);
        return list;
    }
}