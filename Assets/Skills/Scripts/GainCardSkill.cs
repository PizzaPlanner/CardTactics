using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Gain Card")]
public class GainCardSkill : Skill
{
    GameSettings.PlayerInfo Player;

    [SerializeField] Card NewCardPrefab;
    [SerializeField] Card.CardState NewCardInitialState;

    protected override bool OnActivate(GameSettings.PlayerInfo player, Card card)
    {
        Player = player;
        return Settings.Map.ShowRange(SourceTile, 0, 0, TargetType.Ally, RangeType.Any, player) > 0;
    }

    protected override void OnPreview()
    {

    }

    public override void OnUse()
    {
        Player.Deck.ExtendWith(NewCardPrefab, Source, NewCardInitialState);
    }

    protected override void OnCancel()
    {

    }
}