using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GainCardSkill : ISkill
{
    [SerializeField] Card NewCardPrefab;
    [SerializeField] CardState NewCardInitialState;

    public string Description => $"Gain {NewCardPrefab.Name} ({NewCardInitialState})";

    public bool AutoSelectOnOnePossibleTile => true;

    public IEnumerable<ITilePattern> CreatePatterns(SkillContext ctx)
    {
        return new[] { new SingleTilePattern { Target = ctx.SourceTile }.Build(ctx.Map, ctx.SourceTile, new SelectionFilter { Range = RangeFilter.Any }) };
    }

    public IEnumerator ExecuteServer(SkillContext ctx)
    {
        if (ctx.OutTile.Unit == null || !ctx.OutTile.Unit.TryGetComponent<IngameDeck>(out var deck)) yield break;
        deck.Contexts.Add(new CardContext(NewCardPrefab, ctx.OutTile.Unit, NewCardInitialState));
        yield return null;
    }

    public IEnumerator ExecuteClient(SkillContext ctx)
    {
        yield return ShowGetAnimation(NewCardPrefab);
    }

    public static IEnumerator ShowGetAnimation(Card cardPrefab)
    {
        yield return new WaitForEndOfFrame();
        CardTooltip.Show(cardPrefab);
        yield return GameSettings.Instance.CardGainAnnouncerTime.Yield();
        CardTooltip.Hide();
    }

    public void Serialize(Mirror.NetworkWriter writer)
    {
        writer.WriteObject(NewCardPrefab);
    }
    public ISkill Deserialize(Mirror.NetworkReader reader)
    {
        NewCardPrefab = reader.ReadObject<Card>();
        return this;
    }
}