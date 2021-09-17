using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Gain Card")]
public class GainCardSkill : Skill
{
    [NonSerialized] public GameSettings.PlayerInfo Player;

    [SerializeField] Card NewCardPrefab;
    [SerializeField] Card.CardState NewCardInitialState;

    public Card CardPrefab => NewCardPrefab;

    protected override Tile[] OnActivate(GameSettings settings, GameSettings.PlayerInfo player, Card card)
    {
        Player = player;
        return settings.Map.ShowRange(SourceTile, 0, 0, TargetType.Ally, RangeType.Any, player);
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
}