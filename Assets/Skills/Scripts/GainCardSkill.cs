using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GainCardSkill : Skill
{
    [NonSerialized] public PlayerInfo Player;

    [SerializeField] Card NewCardPrefab;
    [SerializeField] Card.CardState NewCardInitialState;

    public Card CardPrefab => NewCardPrefab;

    public override string Description => $"Gain {CardPrefab.Name} ({NewCardInitialState})";

    protected override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        Player = player;
        return new[] { new SingleTilePattern { Target = SourceTile }.Build(map, SourceTile, player, new SelectionFilter { Range = RangeFilter.Any }) };
    }

    protected override IEnumerator OnPreview(ITilePattern pattern)
    {
        OutTile = pattern.Tiles.First();
        yield return ShowGetAnimation(NewCardPrefab);
    }

    public static IEnumerator ShowGetAnimation(Card cardPrefab)
    {
        yield return new WaitForEndOfFrame();
        CardTooltip.Show(cardPrefab);
        yield return new WaitForSeconds(2);
        CardTooltip.Hide();
    }

    protected override void OnUse()
    {
        Player.Deck.ExtendWith(NewCardPrefab, Source, NewCardInitialState);
    }

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.Add(CardPrefab);
        return list;
    }
}