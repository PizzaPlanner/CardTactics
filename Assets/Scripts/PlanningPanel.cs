using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlanningPanel : MonoBehaviour
{
    [SerializeField] RectTransform cardsPanel, equipPanel;
    [SerializeField] CardDisplay cardPrefab;
    [SerializeField] Button confirmButton;
    [SerializeField] RectTransform costBox;
    [SerializeField] RectTransform costPrefab;

    [SerializeField] AudioClip SelectSound, UnselectSound, ReadySound;
    [SerializeField] CardDisplay commanderPanel;

    int RoundPointsSpent;

    AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        Hide();
    }

    public void Show(PlayerInfo player)
    {
        var deck = player.Deck;

        commanderPanel.SetCard(deck.Commander);

        var unplayed = deck.Unplayed.ToList();
        unplayed.Sort((x, y) => x.Name.CompareTo(y.Name));
        confirmButton.enabled = true;
        confirmButton.interactable = false;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            if (!deck.QueueIsValid) return;
            audio.PlayOneShot(ReadySound);
            confirmButton.enabled = false;
            Hide();
            GameObject.FindObjectOfType<Board>().SetPlayerReady(player);
        });

        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in equipPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var card in deck.Equipped)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(equipPanel);
            visual.GetComponent<Button>().interactable = false;
        }

        List<CardDisplay> displayCards = new List<CardDisplay>();
        foreach (var card in unplayed)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(cardsPanel);
            displayCards.Add(visual);
            visual.GetComponent<Button>().onClick.AddListener(() =>
            {
                card.SetSelectionState(!deck.Queued.Contains(card));

                var selected = card.State == Card.CardState.Queued;
                audio.PlayOneShot(selected ? UnselectSound : SelectSound);
                visual.GetComponent<Button>().animator.SetBool("IsActive", selected);
                CalculateValidCards(deck, displayCards);
            });
        }
        CalculateValidCards(deck, displayCards);
        gameObject.SetActive(true);
        StartCoroutine(FixLayoutBug());
    }

    void CalculateValidCards(Deck deck, List<CardDisplay> displayCards)
    {
        var capacities = deck.GetTotalCapacities();
        var uses = deck.GetUsedCapacities();
        var free = deck.GetTotalCapacities();
        foreach (var cost in uses)
            free[cost.Key] -= uses[cost.Key];

        foreach (var otherCard in displayCards)
        {
            if (otherCard.Reference is not ActionCard acard) continue;
            if (deck.Queued.Contains(acard) || CanAfford(acard, uses, capacities))
            {
                otherCard.GetComponent<Button>().interactable = true;
                otherCard.GetComponent<ActionDisplay>().SetCostColor(capacities);
            }
            else
            {
                otherCard.GetComponent<Button>().interactable = false;
                otherCard.GetComponent<ActionDisplay>().SetCostColor(free);
            }
        }

        foreach (Transform t in costBox)
            t.gameObject.SetActive(false);

        int i = 0;
        foreach (var cost in capacities)
        {
            if (cost.Value <= 0) continue;
            var row = costBox.GetChild(i++);
            foreach (Transform t in row)
                Destroy(t.gameObject);
            row.gameObject.SetActive(true);
            for (int j = 0; j < cost.Value; j++)
            {
                var item = Instantiate(costPrefab);
                item.GetComponentInChildren<Image>().sprite = cost.Key.Icon;
                item.SetParent(row);
                if (uses.ContainsKey(cost.Key) && j >= cost.Value - uses[cost.Key])
                    item.GetComponentInChildren<Image>().color *= new Color(1, 1, 1, 0.2f);
            }
        }

        confirmButton.interactable = deck.QueueIsValid;
    }

    private bool CanAfford(ActionCard card, Dictionary<CardClass, int> uses, Dictionary<CardClass, int> capacities)
    {
        return card.Cost.All(cost => capacities[cost.Class] - uses[cost.Class] >= cost.Cost);
    }

    // https://issuetracker.unity3d.com/issues/horizontallayoutgroup-fails-to-position-children-when-entering-slash-leaving-play-mode-if-controlchildsize-dot-height-is-enabled?_ga=2.205791891.1245712443.1631889840-1271266292.1625017527
    IEnumerator FixLayoutBug()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.MarkLayoutForRebuild(cardsPanel);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
