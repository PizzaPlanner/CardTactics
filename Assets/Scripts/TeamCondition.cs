using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class TeamCondition
{
    public abstract bool Check(Map map, Team team);

    [SerializeReference] public TeamTrigger Trigger;
}

[System.Serializable]
public class TeamConditionEveryoneDead : TeamCondition
{
    public override bool Check(Map map, Team team) => !team.RegisteredPlayers.Any(x => x.IsAlive);
}

[System.Serializable]
public class TeamConditionOnlyTeamAlive : TeamCondition
{
    public override bool Check(Map map, Team team)
    {
        var dead = new TeamConditionEveryoneDead();
        return !dead.Check(map, team) && map.Teams.Where(x => x != team).All(x => dead.Check(map, x));
    }
}