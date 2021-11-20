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

    VisualElement cardPanel, planningPanel, unitPanel;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        planningPanel = FindObjectOfType<UIDocument>().rootVisualElement.Q("PlanningPanel");
        Debug.Assert(planningPanel != null, this);
        cardPanel = planningPanel.Q("CardPanel");
        unitPanel = planningPanel.Q("CurrentUnit");
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

        var board = FindObjectOfType<Board>();
        var playable = board.CurrentUnit == unit && unit.Player.IsLocal && board.CurrentStage == Board.Stage.Plan;
        planningPanel.Q<Button>("Confirm").SetEnabled(playable);
        if (playable)
        {
            onEndTurnWrapper ??= onEndTurn;
            onPlayWrapper ??= onPlay;
        }

        var doc = FindObjectOfType<UIDocument>();
        cardPanel.Clear();
        foreach (var card in unplayed)
        {
            cardAsset.CloneTree(cardPanel, out int idx, out int _);
            var cx = cardPanel[idx].Q<CardItem>();
            cx.AddToClassList("carditem-selectable");
            cx.Card = card;
            if (playable)
                cx.RegisterCallback<MouseDownEvent>((_) =>
                {
                    audio.PlayOneShot(SelectSound);
                    OnConfirm(card);
                });
        }
        CalculateValidCards(unit);

        unitPanel.Q<UnitItem>().Unit = unit;
        unitPanel.Q<ResourcePanel>().Resources = unit.Resources;

        planningPanel.style.display = DisplayStyle.Flex;
    }

    void CalculateValidCards(Unit unit)
    {
        cardPanel.Query<CardItem>().ForEach(x =>
        {
            if (x.Card is ActionCard card)
            {
                x.Query<ResourcePanel>().ForEach(x => x.SetAffordable(unit.Resources));
                x.SetEnabled(unit.Resources.CanAfford(card.Cost));
            }
        });
    }
}
