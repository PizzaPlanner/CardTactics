using UnityEngine;

public abstract class LooseCondition : ScriptableObject
{
    public abstract bool Check(GameSettings settings, GameSettings.PlayerInfo player);
}