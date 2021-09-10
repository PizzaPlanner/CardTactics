using UnityEngine;
using UnityEngine.UI;

public class PlayerPendingBadge : MonoBehaviour
{
    public Deck Deck;
    public Color Color => Board.PlayerColor(Deck.Player);

    void Start()
    {
        GetComponentInChildren<Image>().color = Color;
        transform.SetSiblingIndex(Deck.Player);
    }
}