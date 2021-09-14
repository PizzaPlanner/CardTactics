using UnityEngine;
using UnityEngine.UI;

public class PlayerPendingBadge : MonoBehaviour
{
    public GameSettings.PlayerInfo Player;
    public bool IsReady;
    public bool IsDead;

    void Start()
    {
        GetComponentInChildren<Image>().color = Player.Color;
    }
}