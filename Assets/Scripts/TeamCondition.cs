using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamConditions
{

    [System.Serializable]
    public abstract class TeamCondition
    {
        public abstract bool Check(Map map, Team team);

        public Trigger OnConditionMet;
    }

    [System.Serializable]
    public class EveryoneDead : TeamCondition
    {
        public override bool Check(Map map, Team team) => !Unit.FindObjectsOfType<Unit>(true).Any(x => !x.IsDead && map.Teams[x.Player.Team] == team);
    }

    [System.Serializable]
    public class OnlyTeamAlive : TeamCondition
    {
        public override bool Check(Map map, Team team)
        {
            var dead = new EveryoneDead();
            return !dead.Check(map, team) && map.Teams.Where(x => x != team).All(x => dead.Check(map, x));
        }
    }

    [System.Serializable]
    public class AllAreasUncovered : TeamCondition
    {
        public override bool Check(Map map, Team team)
        {
            return map.InvisibleTiles.Count() == 0;
        }
    }

    [System.Serializable]
    public class OnlyTeamAliveAndAllAreasUncovered : TeamCondition
    {
        public override bool Check(Map map, Team team)
        {
            return new AllAreasUncovered().Check(map, team) && new OnlyTeamAlive().Check(map, team);
        }
    }
}