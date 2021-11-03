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
        planningPanel.Q<Button>("Confirm").clicked += OnEndTurn;
        Hide();
    }

    public void Hide()
    {
        planningPanel.style.display = DisplayStyle.None;
    }

    void OnEndTurn()
    {
        onEndTurnWrapper.Invoke();
        Hide();
    }

    void OnConfirm(Card card)
    {
        onPlayWrapper.Invoke(card);
        Hide();
    }

    Action<Card> onPlayWrapper;
    Action onEndTurnWrapper;

    public void Show(PlayerInfo player, Unit unit, Action<Card> onPlay, Action onEndTurn)
    {
        var deck = player.Deck;

        var unplayed = deck.Unplayed.ToList();
        unplayed.Sort((x, y) => x.Name.CompareTo(y.Name));

        onPlayWrapper = onPlay;
        onEndTurnWrapper = onEndTurn;

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
                OnConfirm(card);
            });
        }
        CalculateValidCards(unit);

        planningPanel.style.display = DisplayStyle.Flex;
    }

    void CalculateValidCards(Unit unit)
    {
        cardPanel.Query<CardItem>().ForEach(x =>
        {
            if (x.Card is ActionCard card)
            {
                var energy = unit.Resources.Energy - unit.Resources.EnergySpent;
                var hasEnergy = energy >= card.Cost.Energy;
                x.Query<ResourcePanel>().ForEach(x => x.SetAffordable(unit.Resources));
                x.Query<Label>("Energy").ForEach(x =>
                {
                    x.text = card.Cost.Energy.ToString();
                    if (hasEnergy)
                        x.RemoveFromClassList("text-color-overbudget");
                    else
                        x.AddToClassList("text-color-overbudget");
                });
                x.SetEnabled(unit.Resources.CanAfford(card.Cost));
            }
        });
    }
}
