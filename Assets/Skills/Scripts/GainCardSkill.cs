using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GainCardSkill : Skill
{
    [SerializeField] Card NewCardPrefab;
    [SerializeField] CardState NewCardInitialState;

    public override string Description => $"Gain {NewCardPrefab.Name} ({NewCardInitialState})";

    public override bool AutoSelectOnOnePossibleTile => true;

    protected override IEnumerable<ITilePattern> OnActivate(SkillContext ctx)
    {
        return new[] { new SingleTilePattern { Target = ctx.SourceTile }.Build(ctx.Source.Map, ctx.SourceTile, new SelectionFilter { Range = RangeFilter.Any }) };
    }

    public override IEnumerator OnPreview(SkillContext ctx)
    {
        ctx.OutTile = ctx.SkillTarget.Tiles.First();
        if (ctx.OutTile.Unit == null || !ctx.OutTile.Unit.TryGetComponent<IngameDeck>(out var deck)) yield break;
        yield return ShowGetAnimation(NewCardPrefab);
        deck.Contexts.Add(new CardContext(NewCardPrefab, Map.FindObjectOfType<Map>(), ctx.OutTile.Unit, NewCardInitialState));
    }

    public static IEnumerator ShowGetAnimation(Card cardPrefab)
    {
        yield return new WaitForEndOfFrame();
        CardTooltip.Show(cardPrefab);
        yield return GameSettings.Instance.CardGainAnnouncerTime.Yield();
        CardTooltip.Hide();
    }

    public override List<Card> GetRelatedCards()
    {
        var list = base.GetRelatedCards();
        list.Add(NewCardPrefab);
        return list;
    }
}