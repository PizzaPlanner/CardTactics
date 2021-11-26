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
    [SerializeField] UIDocument uiRoot;
    void Awake()
    {
        audio = GetComponent<AudioSource>();
        planningPanel = uiRoot.rootVisualElement.Q("PlanningPanel");
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

    void OnConfirm(CardContext card)
    {
        onPlayWrapper.Invoke(card);
        Hide();
    }

    Action<CardContext> onPlayWrapper;
    Action onEndTurnWrapper;

    public void Show(Unit unit, Action<CardContext> onPlay, Action onEndTurn)
    {
        if (!unit.TryGetComponent<IngameDeck>(out var deck)) return;

        var unplayed = deck.Unplayed.ToList();
        unplayed.Sort((x, y) => x.Target.Name.CompareTo(y.Target.Name));

        var board = FindObjectOfType<Board>();
        var playable = board.CurrentUnit == unit && unit.Player.Identity.isLocalPlayer && board.CurrentStage == Board.Stage.Plan;
        planningPanel.Q<Button>("Confirm").SetEnabled(playable);
        if (playable)
        {
            onEndTurnWrapper ??= onEndTurn;
            onPlayWrapper ??= onPlay;
        }

        var doc = uiRoot;
        cardPanel.Clear();
        var map = FindObjectOfType<Map>();
        foreach (var ctx in unplayed)
        {
            cardAsset.CloneTree(cardPanel, out int idx, out int _);
            var item = cardPanel[idx].Q<CardItem>();
            item.AddToClassList("carditem-selectable");
            item.Card = ctx.Target;
            var firstSkill = ctx.Target.skills.FirstOrDefault();
            if (firstSkill != null)
            {
                new SkillContext(firstSkill, ctx, map.TileOf(unit)).Preview(out var validPatterns, out var invalidPatterns);
                item.OnMouseEnter = () =>
                {
                    CardTooltip.Show(ctx.Target);
                    map.ShowTileIndicators(validPatterns, (pattern) => TargetIndicator.Preview);
                    map.ShowTileIndicators(invalidPatterns, (pattern) => TargetIndicator.Invalid);
                };
            }
            else
                item.OnMouseEnter = () => { CardTooltip.Show(ctx.Target); };

            item.OnMouseLeave = () => { CardTooltip.Hide(); map.HideTileSelectionPreview(); };

            if (playable)
                item.RegisterCallback<MouseDownEvent>((_) =>
                {
                    audio.PlayOneShot(SelectSound);
                    OnConfirm(ctx);
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
