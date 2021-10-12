using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlanningPanel : MonoBehaviour
{
    [SerializeField] RectTransform cardsPanel, equipPanel;
    [SerializeField] CardDisplay cardPrefab;
    [SerializeField] Button confirmButton;
    [SerializeField] Text capacityText;

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
        unplayed.AddRange(deck.Queued);
        unplayed.Sort((x, y) => x.Name.CompareTo(y.Name));
        confirmButton.enabled = true;
        confirmButton.interactable = false;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
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

        foreach (var card in unplayed)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(cardsPanel);
            if (deck.Queued.Contains(card))
            {
                visual.GetComponent<Button>().animator.SetBool("IsLocked", true);
            }
            else
                visual.GetComponent<Button>().onClick.AddListener(() =>
                {
                    card.SetSelectionState(!deck.Queued.Contains(card));

                    var selected = card.State == Card.CardState.Queued;
                    audio.PlayOneShot(selected ? UnselectSound : SelectSound);
                    visual.GetComponent<Button>().animator.SetBool("IsActive", selected);
                    CalculateValidCards(deck);
                });
        }
        CalculateValidCards(deck);
        gameObject.SetActive(true);
        StartCoroutine(FixLayoutBug());
    }

    void CalculateValidCards(Deck deck)
    {
        var capacities = deck.GetCapacities();
        var uses = new Dictionary<CardClass, int>();
        var classes = new List<CardClass>();
        foreach (var key in capacities.Keys)
            uses[key] = 0;
        foreach (var card in deck.Queued)
        {
            if (card is not ActionCard acard) continue;
            uses[acard.RequiredClass] += acard.Cost;
            if (!classes.Contains(acard.RequiredClass))
                classes.Add(acard.RequiredClass);
        }

        foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardDisplay>(false))
        {
            if (otherCard.Reference is not ActionCard acard) continue;
            if (!capacities.ContainsKey(acard.RequiredClass))
                capacities[acard.RequiredClass] = 0;
            if (!uses.ContainsKey(acard.RequiredClass))
                uses[acard.RequiredClass] = 0;
            if (deck.Queued.Contains(acard) ||
            (capacities[acard.RequiredClass] - uses[acard.RequiredClass] >= acard.Cost
            && (classes.Contains(acard.RequiredClass) || classes.Count < deck.ClassesPerTurn)))
            {
                otherCard.GetComponent<Button>().interactable = true;
            }
            else
            {
                otherCard.GetComponent<Button>().interactable = false;
            }
        }

        var countText = $"Categories: {classes.Count}/{deck.ClassesPerTurn}\nSpeed: {deck.CurrentSpeed}\n-----\n";
        foreach (var cap in capacities.Keys)
        {
            if (capacities[cap] == 0) continue;
            countText += $"{cap.Name}: {uses[cap]}/{capacities[cap]}\n";
        }
        capacityText.text = countText.Trim();
        confirmButton.interactable = deck.Queued.Count() != 0;
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
