using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Passive : Skill
{
    public enum TargetType
    {
        Unit,
        Player
    }

    protected override bool AutoSelectOnOnePossibleTile => true;
    public int NumberOfRounds => numberOfRounds;
    [SerializeField] int numberOfRounds;
    [NonSerialized] public int RoundsLeft;
    protected abstract TargetType Target { get; }

    protected Unit TargetUnit { get; private set; }
    protected PlayerInfo TargetPlayer { get; private set; }

    public virtual void OnRoundStart() { }
    public virtual void OnActivation() { }

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        if (Target == TargetType.Player)
            TargetPlayer = player;
        else if (Target == TargetType.Unit && SourceTile.Unit == null)
        {
            Debug.LogError("No unit on Tile", SourceTile);
            return new ITilePattern[0];
        }

        return new[] { new SingleTilePattern { Target = SourceTile }.Build(map, SourceTile, player, SelectionFilters.Everything) };
    }

    public virtual void OnPassiveEnd() { }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        if (Target == TargetType.Unit)
            TargetUnit = targetPattern.Tiles.First().Unit;
        RoundsLeft = NumberOfRounds;
        OnActivation();
        yield return null;
    }

    protected override void OnUse()
    {
    }
}