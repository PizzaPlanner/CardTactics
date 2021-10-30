using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
// using UnityEngine.UIElements;

[RequireComponent(typeof(AudioSource))]
public class PlanningPanel : MonoBehaviour
{
    [SerializeField] AudioClip SelectSound, UnselectSound, ReadySound;
    [SerializeField] VisualTreeAsset cardAsset;

    AudioSource audio;

    VisualElement cardPanel, planningPanel, resourcePanel;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        planningPanel = FindObjectOfType<UIDocument>().rootVisualElement.Q("PlanningPanel");
        Debug.Assert(planningPanel != null, this);
        cardPanel = planningPanel.Q("CardPanel");
        planningPanel.Q<Button>("Confirm").clicked += OnConfirm;
        Hide();
    }

    public void Hide()
    {
        planningPanel.style.display = DisplayStyle.None;
    }


    void OnConfirm()
    {
        audio.PlayOneShot(ReadySound);
        onConfirmWrapper?.Invoke();
        Hide();
    }
    Action onConfirmWrapper;

    public void Show(PlayerInfo player, Action onConfirm)
    {
        var deck = player.Deck;

        var badges = FindObjectOfType<UIDocument>().rootVisualElement.Query<PlayerInfoBadge>()
        .Where(x => x.Player == player).ToList();

        var unplayed = deck.Unplayed.ToList();
        unplayed.Sort((x, y) => x.Name.CompareTo(y.Name));

        onConfirmWrapper = onConfirm;

        var doc = FindObjectOfType<UIDocument>();
        cardPanel.Clear();
        foreach (var card in unplayed)
        {
            cardAsset.CloneTree(cardPanel, out int idx, out int _);
            var cx = cardPanel[idx].Q<CardItem>();
            cx.AddToClassList("carditem-selectable");
            cx.Card = card;
            cx.RegisterCallback<MouseDownEvent>((_) =>
            {
                card.SetSelectionState(!deck.Queued.Contains(card));
                var selected = card.State == Card.CardState.Queued;
                audio.PlayOneShot(selected ? UnselectSound : SelectSound);
                if (selected)
                    cx.AddToClassList("carditem-selectable--selected");
                else
                    cx.RemoveFromClassList("carditem-selectable--selected");
                CalculateValidCards(deck, badges);
            });
        }
        CalculateValidCards(deck, badges);

        planningPanel.style.display = DisplayStyle.Flex;
    }

    void CalculateValidCards(Deck deck, List<PlayerInfoBadge> badges)
    {
        var capacities = deck.GetTotalCapacities();
        var uses = deck.GetUsedCapacities();
        var free = deck.GetTotalCapacities();
        foreach (var cost in uses)
            free[cost.Key] -= uses[cost.Key];

        badges.ForEach(x => x.Query<CostIcon>().ForEach(y => y.SetCostColor(free)));

        cardPanel.Query<CardItem>().ForEach(x =>
        {
            if (x.Card is ActionCard card)
            {
                var costOk = true;
                if (!x.ClassListContains("carditem-selectable--selected"))
                    foreach (var cost in card.Cost)
                        costOk &= free[cost.Class] >= cost.Cost;

                x.Query<CostIcon>().ForEach(x => x.SetCostColor(costOk ? capacities : free));
                x.SetEnabled(costOk);
            }
        });

        planningPanel.Q<Button>("Confirm").SetEnabled(deck.QueueIsValid);
    }
}
