using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlanningPanel : MonoBehaviour
{
    [SerializeField] RectTransform cardsPanel;
    [SerializeField] CardDisplay cardPrefab;
    [SerializeField] TurnPlate turnPlate;

    [SerializeField] AudioClip SelectSound, UnselectSound, ReadySound;

    PlayerInfoPanel infoPanel;

    int RoundPointsSpent;

    AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        Hide();
    }

    public void Show(PlayerInfo player, PlayerInfoPanel panel)
    {
        var deck = player.Deck;

        var selection = new List<Card>();
        var cardsToPlay = new List<Card>();
        var cardsPlayed = new List<Card>();
        var cardsDestroyed = new List<Card>();
        foreach (var card in deck.Cards)
        {
            switch (card.State)
            {
                case Card.CardState.Unplayed:
                    cardsToPlay.Add(card); break;
                case Card.CardState.Active:
                case Card.CardState.Played:
                    cardsPlayed.Add(card); break;
                case Card.CardState.Destroyed:
                    cardsDestroyed.Add(card); break;
            }
        }
        cardsToPlay.Sort((x, y) => x.Speed.CompareTo(y.Speed));
        RoundPointsSpent = 0;
        infoPanel = panel;
        infoPanel.confirmButton.GetComponentInChildren<Text>().text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
        infoPanel.confirmButton.enabled = true;
        infoPanel.confirmButton.interactable = false;

        infoPanel.confirmButton.onClick.RemoveAllListeners();
        infoPanel.confirmButton.onClick.AddListener(() =>
        {
            audio.PlayOneShot(ReadySound);
            Hide();
            turnPlate.SetPlayerReady(player, selection.ToArray());
            infoPanel.confirmButton.enabled = false;
        });

        foreach (Transform child in cardsPanel)
        {
            Destroy(child.gameObject);
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
                    RoundPointsSpent -= card.Cost;
                }
                else
                {
                    RoundPointsSpent += card.Cost;
                    selection.Add(card);
                }
                audio.PlayOneShot(selection.Contains(card) ? UnselectSound : SelectSound);
                visual.GetComponent<Button>().animator.SetBool("IsActive", selection.Contains(card));
                infoPanel.confirmButton.interactable = selection.Count != 0 && RoundPointsSpent <= deck.RoundPointsFree;
                infoPanel.confirmButton.GetComponentInChildren<Text>().text = $"{RoundPointsSpent}/{deck.RoundPointsFree}";
                foreach (var otherCard in cardsPanel.GetComponentsInChildren<CardDisplay>(false))
                {
                    if (selection.Contains(otherCard.Reference) || RoundPointsSpent + otherCard.Reference.Cost <= deck.RoundPointsFree)
                    {
                        otherCard.GetComponent<Button>().interactable = true;
                        otherCard.Cost.color = Color.white;
                    }
                    else
                    {
                        otherCard.GetComponent<Button>().interactable = false;
                        otherCard.Cost.color = Color.red;
                    }
                }
            });
        }

        gameObject.SetActive(true);
        StartCoroutine(FixLayoutBug());
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
