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

    VisualElement cardPanel, planningPanel, unitPanel, unplayablePanel;
    [SerializeField] UIDocument uiRoot;
    void Awake()
    {
        audio = GetComponent<AudioSource>();
        planningPanel = uiRoot.rootVisualElement.Q("PlanningPanel");
        Debug.Assert(planningPanel != null, this);
        cardPanel = planningPanel.Q("CardPanel");
        unplayablePanel = planningPanel.Q("UnplayablePanel");
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

    void OnConfirm(int cardInDeck)
    {
        onPlayWrapper.Invoke(cardInDeck);
        Hide();
    }

    Action<int> onPlayWrapper;
    Action onEndTurnWrapper;

    public void Show(Unit unit, Action<int> onPlay, Action onEndTurn)
    {
        if (!unit.TryGetComponent<IngameDeck>(out var deck)) return;

        var board = FindObjectOfType<Board>();

        var isCurrent = GameStateMachine.FindObjectOfType<GameStateMachine>().CurrentUnit == unit;
        var playable = isCurrent && PlayerController.Local.ControlledUnits.Contains(unit);
        if (playable && onPlay != null)
            onPlayWrapper = onPlay;
        var confirmable = isCurrent && PlayerController.Local.ControlledUnits.Contains(unit);
        if (confirmable && onEndTurn != null)
            onEndTurnWrapper = onEndTurn;
        planningPanel.Q<Button>("Confirm").SetEnabled(confirmable);

        var doc = uiRoot;
        cardPanel.Clear();
        unplayablePanel.Clear();
        var map = FindObjectOfType<Map>();
        var unplayed = deck.Unplayed.ToList();
        unplayed.Sort((x, y) => x.Target.Name.CompareTo(y.Target.Name));
        foreach (var ctx in unplayed)
        {
            cardAsset.CloneTree(cardPanel, out int idx, out int _);
            var item = cardPanel[idx].Q<CardItem>();
            item.AddToClassList("carditem-selectable");
            item.Card = ctx.Target;
            var firstSkill = ctx.Target.skills.FirstOrDefault();
            if (firstSkill != null)
            {
                new SkillContext(firstSkill, unit, map, map.TileOf(unit)).Preview(out var validPatterns, out var invalidPatterns);
                item.OnMouseEnter = () =>
                {
                    CardTooltip.Show(ctx.Target);
                    map.ShowPatterns(validPatterns, false);
                };
            }
            else
                item.OnMouseEnter = () => { CardTooltip.Show(ctx.Target); };

            item.OnMouseLeave = () => { CardTooltip.Hide(); map.HideTileSelectionPreview(); };

            if (playable)
                item.RegisterCallback<MouseDownEvent>((_) =>
                {
                    audio.PlayOneShot(SelectSound);
                    OnConfirm(deck.IndexOf(ctx));
                });
        }
        CalculateValidCards(unit);

        var unplayable = deck.Unplayable.ToList();
        unplayable.Sort((x, y) => x.Target.Name.CompareTo(y.Target.Name));
        foreach (var ctx in unplayable)
        {
            cardAsset.CloneTree(unplayablePanel, out int idx, out int _);
            var item = unplayablePanel[idx].Q<CardItem>();
            item.Card = ctx.Target;

            item.OnMouseEnter = () => { CardTooltip.Show(ctx.Target); };
            item.OnMouseLeave = () => { CardTooltip.Hide(); };
        }

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
