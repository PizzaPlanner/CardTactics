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
    [SerializeField] CardDisplay itemCardPrefab;

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
        commanderCard.Speed.text = Player.Deck.CurrentSpeed.ToString();
        commanderCard.GetComponent<Button>().interactable = false;
        foreach (Transform t in passivePanel)
            GameObject.Destroy(t.gameObject);

        foreach (var card in Player.Deck.Equipped)
        {
            var item = Instantiate(itemCardPrefab);
            item.SetCard(card);
            item.transform.SetParent(passivePanel);
        }
    }
}