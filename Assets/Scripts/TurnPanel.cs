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

    [SerializeField] float turnAnnouncerTime;

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

        badge.UnitItem.Unit = unit;
        var border = badge.UnitItem.Q("Border");
        border.style.borderTopColor = player.Color;
        border.style.borderLeftColor = player.Color;
        border.style.borderRightColor = player.Color;
        border.style.borderBottomColor = player.Color;
        var resources = badge.Q<ResourcePanel>();
        resources.Deck = player.Deck;
        OnTurnEnd(unit);

        SortBadges();
    }

    internal void SortBadges()
    {
        readyPanel.Sort((x, y) => x.Q<UnitItem>().Unit.Speed.CompareTo(y.Q<UnitItem>().Unit.Speed));
    }

    internal void RemoveUnit(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.Q<UnitItem>().Unit == unit).ForEach(x => x.RemoveFromHierarchy());
    }

    internal IEnumerator OnTurnStart(Unit unit, Action onComplete)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.SetEnabled(true));
        turnAnnouncer.Q<Label>().text = $"Turn of <color=#{ColorUtility.ToHtmlStringRGB(unit.Player.Color)}>{unit.Name}</color>";
        turnAnnouncer.style.display = DisplayStyle.Flex;
        yield return new WaitForSeconds(turnAnnouncerTime);
        turnAnnouncer.style.display = DisplayStyle.None;
        onComplete?.Invoke();
    }

    internal void OnTurnEnd(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.SetEnabled(false));
    }
}
