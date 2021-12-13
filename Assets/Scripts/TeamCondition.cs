using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class TeamCondition
{
    public abstract bool Check(Map map, Team team);

    public Trigger OnConditionMet;
}

[System.Serializable]
public class TeamConditionEveryoneDead : TeamCondition
{
    public override bool Check(Map map, Team team) => !Unit.FindObjectsOfType<Unit>(true).Any(x => !x.IsDead && map.Teams[x.Player.Team] == team);
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