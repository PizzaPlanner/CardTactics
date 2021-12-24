using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[System.Serializable]
public class PlaceSkill : ISkill
{
    [SerializeField] public Unit reference;
    int Team;

    public string Description => null;

    public bool AutoSelectOnOnePossibleTile => true;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        if (reference != null)
        {
            Team = reference.Player.Team;
            reference.Spawn();
        }
        return new SpawnPatternBuilder { Team = Team }.Patterns(Map.Instance, ctx.SourceTile, SelectionFilters.Empty);
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        var facing = ctx.OutTile.TryGetComponent<SpawnLocation>(out var sl) ? sl.Facing : 0;
        RoomNetworkManager.Playables.InsertFirst(new PlaceUnitContext { Unit = reference, Target = ctx.OutTile, Facing = facing });
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteInt(Team);
    }

    public ISkill Deserialize(NetworkReader reader)
    {
        Team = reader.ReadInt();
        return this;
    }
}