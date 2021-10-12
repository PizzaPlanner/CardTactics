using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlanningPanel : MonoBehaviour
{
    [SerializeField] RectTransform cardsPanel, equipPanel;
    [SerializeField] CardDisplay cardPrefab;
    [SerializeField] TurnPlate turnPlate;
    [SerializeField] Text capacityText;

    [SerializeField] AudioClip SelectSound, UnselectSound, ReadySound;

    int RoundPointsSpent;

    AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        Hide();
    }

    public void Show(PlayerInfo player, PlayerInfoPanel infoPanel)
    {
        var deck = player.Deck;

        var selection = new List<ActionCard>();
        var cardsToPlay = new List<ActionCard>();
        var cardsPlayed = new List<Card>();
        var cardsDestroyed = new List<Card>();
        var cardsEquipped = new List<ItemCard>();
        foreach (var card in deck.Cards)
        {
            if (card is ItemCard icard)
                cardsEquipped.Add(icard);
        }
        foreach (var card in deck.Cards)
        {
            if (card is not ActionCard acard || cardsEquipped.Count(x => x.ItemClass == acard.RequiredClass) == 0)
                continue;

            switch (card.State)
            {
                case Card.CardState.Unplayed:
                    cardsToPlay.Add(acard); break;
                case Card.CardState.Active:
                case Card.CardState.Played:
                    cardsPlayed.Add(card); break;
                case Card.CardState.Destroyed:
                    cardsDestroyed.Add(card); break;
            }
        }
        cardsToPlay.Sort((x, y) => x.Name.CompareTo(y.Name));
        RoundPointsSpent = 0;
        infoPanel.confirmButton.GetComponentInChildren<Text>().text = $"{RoundPointsSpent}";
        infoPanel.confirmButton.enabled = true;
        infoPanel.confirmButton.interactable = false;

        infoPanel.confirmButton.onClick.RemoveAllListeners();
        infoPanel.confirmButton.onClick.AddListener(() =>
        {
            audio.PlayOneShot(ReadySound);
            infoPanel.confirmButton.enabled = false;
            Hide();
            GameObject.FindObjectOfType<Board>().SetPlayerReady(player, selection.ToArray());
        });

        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in equipPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var card in cardsEquipped)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(equipPanel);
            visual.GetComponent<Button>().interactable = false;
        }

        foreach (var card in cardsToPlay)
        {
            var visual = Instantiate(cardPrefab);
            visual.SetCard(card);
            visual.transform.SetParent(cardsPanel);
            visual.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selection.Contains(card))
                {
                    selection.Remove(card);
                }
                else
                {
                    selection.Add(card);
                }
                audio.PlayOneShot(selection.Contains(card) ? UnselectSound : SelectSound);
                visual.GetComponent<Button>().animator.SetBool("IsActive", selection.Contains(card));
                infoPanel.confirmButton.interactable = selection.Count != 0;
                infoPanel.confirmButton.GetComponentInChildren<Text>().text = $"{RoundPointsSpent}";
                CalculateValidCards(deck, selection);
            });
        }
        CalculateValidCards(deck, selection);
        gameObject.SetActive(true);
        StartCoroutine(FixLayoutBug());
    }

    void CalculateValidCards(Deck deck, List<ActionCard> selection)
    {
        var capacities = deck.GetCapacities();
        var uses = new Dictionary<CardClass, int>();
        var classes = new List<CardClass>();
        foreach (var key in capacities.Keys)
            uses[key] = 0;
        foreach (var card in selection)
        {
            uses[card.RequiredClass] += card.Cost;
            if (!classes.Contains(card.RequiredClass))
                classes.Add(card.RequiredClass);
        }


        foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardDisplay>(false))
        {
            if (otherCard.Reference is not ActionCard acard) continue;
            if (selection.Contains(acard) ||
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

        var countText = $"Categories: {classes.Count}/{deck.ClassesPerTurn}\n";
        foreach (var cap in capacities.Keys)
        {
            if (capacities[cap] == 0) continue;
            countText += $"{cap.Name}: {uses[cap]}/{capacities[cap]}\n";
        }
        capacityText.text = countText.Trim();
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
