using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    public PlayerInfo Player;
    [SerializeField] Text Name;

    [SerializeField] RectTransform readyBadge;
    [SerializeField] RectTransform passivePanel;
    [SerializeField] CardDisplay commanderCard;

    public bool IsReady
    {
        get => ready; set
        {
            readyBadge.gameObject.SetActive(!Player.IsAI && value);
            ready = value;
        }
    }
    bool ready;

    public void UpdateValues()
    {
        Name.color = Player.Color;
        Name.text = Player.Deck.Commander.Name;
        commanderCard.SetCard(Player.Deck.Commander);
        commanderCard.GetComponent<Button>().interactable = false;

        foreach (Transform t in passivePanel)
            GameObject.Destroy(t.gameObject);
    }
}