using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] VisualTreeAsset readyBadgeAsset;
    [SerializeField] VisualTreeAsset unitStatusBar, unitStatusBarBuffItem;
    VisualElement readyPanel;
    VisualElement turnAnnouncer;

    [SerializeField] UIDocument uiRoot;

    void Awake()
    {
        readyPanel = uiRoot.rootVisualElement.Q("ReadyPanel");
        Debug.Assert(readyPanel != null, this);
        turnAnnouncer = uiRoot.rootVisualElement.Q("TurnAnnouncer");
        Debug.Assert(turnAnnouncer != null, this);
        // foreach (Transform t in nextActionsPanel)
        //     Destroy(t.gameObject);
        turnAnnouncer.style.display = DisplayStyle.None;
    }

    public UnitStatusBar MakeStatusBar()
    {
        var attachments = uiRoot.rootVisualElement.Q("Attachments");
        unitStatusBar.CloneTree(attachments, out var idx, out var _);
        var item = attachments[idx].Q<UnitStatusBar>();
        item.CardPrefab = unitStatusBarBuffItem;
        return item;
    }

    public void AddUnit(Unit unit)
    {
        readyBadgeAsset.CloneTree(readyPanel, out int idx, out int _);
        var badge = readyPanel[idx].Q<PlayerInfoBadge>();
        var player = unit.Player;
        badge.RegisterCallback<MouseDownEvent>((_) =>
        {
            GameObject.FindObjectOfType<PlanningPanel>(false).Show(unit, null, null);
        });

        badge.UnitItem.Unit = unit;
        var border = badge.UnitItem.Q("Border");
        border.style.borderTopColor = player.TeamColor;
        border.style.borderLeftColor = player.TeamColor;
        border.style.borderRightColor = player.TeamColor;
        border.style.borderBottomColor = player.TeamColor;
        badge.SetEnabled(false);
        OnTurnEnd(unit);
    }

    internal void SortBadges()
    {
        readyPanel.Sort((x, y) => Unit.CompareSpeed(x.Q<UnitItem>().Unit, y.Q<UnitItem>().Unit));
    }

    internal void RemoveUnit(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.Q<UnitItem>().Unit == unit).ForEach(x => x.RemoveFromHierarchy());
    }

    internal IEnumerator OnRoundStart(int round, Action onComplete)
    {
        turnAnnouncer.Q<Label>().text = $"Round {round}";
        turnAnnouncer.style.display = DisplayStyle.Flex;
        yield return GameSettings.Instance.TurnAnnouncerTime.Yield();
        turnAnnouncer.style.display = DisplayStyle.None;
        readyPanel.Query<PlayerInfoBadge>().ForEach(x =>
        {
            x.SetEnabled(true);
        });
        SortBadges();
        onComplete?.Invoke();
    }

    internal IEnumerator OnTurnStart(Unit unit, Action onComplete)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.AddToClassList("unit-badge--current"));
        turnAnnouncer.Q<Label>().text = $"Turn of {unit.Name}";
        turnAnnouncer.style.display = DisplayStyle.Flex;
        yield return GameSettings.Instance.TurnAnnouncerTime.Yield();
        turnAnnouncer.style.display = DisplayStyle.None;
        unit.GetComponentInChildren<StatusBar>().Focus = true;
        onComplete?.Invoke();
    }

    internal void OnTurnEnd(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.RemoveFromClassList("unit-badge--current"));
        unit.GetComponentInChildren<StatusBar>().Focus = false;
    }

    internal void OnHover(Unit unit, bool hover)
    {
        readyPanel.Query<PlayerInfoBadge>()
            .Where(x => x.UnitItem.Unit == unit)
            .ForEach(x =>
            {
                if (hover)
                    x.AddToClassList("unit-badge--hovered");
                else
                    x.RemoveFromClassList("unit-badge--hovered");
            });
        unit.GetComponentInChildren<StatusBar>().Focus = hover;
    }
}
