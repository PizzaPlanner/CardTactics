using UnityEngine;

public abstract class LooseCondition : ScriptableObject
{
    public abstract bool Check(Map map, Team team);
}