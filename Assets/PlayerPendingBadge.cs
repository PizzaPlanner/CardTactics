using UnityEngine;
using UnityEngine.UI;

public class PlayerPendingBadge : MonoBehaviour
{
    public GameSettings.PlayerInfo Player;

    void Start()
    {
        GetComponentInChildren<Image>().color = Player.Color;
    }
}