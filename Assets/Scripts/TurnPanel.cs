using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] VisualTreeAsset readyBadgeAsset;
    VisualElement readyPanel;
    VisualElement turnAnnouncer;

    void Awake()
    {
        readyPanel = FindObjectOfType<UIDocument>().rootVisualElement.Q("ReadyPanel");
        Debug.Assert(readyPanel != null, this);
        turnAnnouncer = FindObjectOfType<UIDocument>().rootVisualElement.Q("TurnAnnouncer");
        Debug.Assert(turnAnnouncer != null, this);
        // foreach (Transform t in nextActionsPanel)
        //     Destroy(t.gameObject);
        turnAnnouncer.style.display = DisplayStyle.None;
    }

    public void AddUnit(Unit unit)
    {
        readyBadgeAsset.CloneTree(readyPanel, out int idx, out int _);
        var badge = readyPanel[idx].Q<PlayerInfoBadge>();
        var player = unit.Player;
        badge.RegisterCallback<MouseDownEvent>((_) =>
        {
            GameObject.FindObjectOfType<PlanningPanel>(false).Show(player, unit, null, null);
        });

        badge.UnitItem.Unit = unit;
        var border = badge.UnitItem.Q("Border");
        border.style.borderTopColor = player.Team.Color;
        border.style.borderLeftColor = player.Team.Color;
        border.style.borderRightColor = player.Team.Color;
        border.style.borderBottomColor = player.Team.Color;
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

    public void OnPlayablePlayed()
    {
        // readyPanel.Query<PlayerInfoBadge>().ForEach(x =>
        // {
        //     var res = x.UnitItem.Unit.Resources;
        //     x.Q<ResourcePanel>().Resources = res;
        // });
    }

    internal IEnumerator OnTurnStart(Unit unit, Action onComplete)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.AddToClassList("unit-badge--current"));
        turnAnnouncer.Q<Label>().text = $"Turn of <color=#{ColorUtility.ToHtmlStringRGB(unit.Player.Team.Color)}>{unit.Name}</color>";
        turnAnnouncer.style.display = DisplayStyle.Flex;
        yield return GameSettings.Instance.TurnAnnouncerTime.Yield();
        turnAnnouncer.style.display = DisplayStyle.None;
        onComplete?.Invoke();
    }

    internal void OnTurnEnd(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.RemoveFromClassList("unit-badge--current"));
    }
}
