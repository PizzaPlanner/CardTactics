using UnityEngine;

public abstract class Passive : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;

    public Card Source;

    public abstract void OnRoundStart(GameSettings settings, GameSettings.PlayerInfo player);
}