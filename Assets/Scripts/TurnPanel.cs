using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] VisualTreeAsset readyBadgeAsset;
    VisualElement readyPanel;

    void Awake()
    {
        readyPanel = FindObjectOfType<UIDocument>().rootVisualElement.Q("ReadyPanel");
        Debug.Assert(readyPanel != null, this);

        // foreach (Transform t in nextActionsPanel)
        //     Destroy(t.gameObject);
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
    }

    public void OnRoundStart()
    {

    }

    public void OnPreparation()
    {
        // readyPanel.Query<PlayerInfoBadge>()
        // .ForEach(x => x.Q<ResourcePanel>().Deck = x.Player.Deck);
        // foreach (Transform t in nextActionsPanel)
        //     Destroy(t.gameObject);
    }

    public void OnCardInQueue(Card card, PlayerInfo player)
    {
        // var indicator = Instantiate(turnIndicatorPrefab);
        // indicator.GetComponent<SkillProgressBar>().SetSkills(card, player);
        // indicator.SetCard(card);
        // // indicator.PlayerColorBorder.color = player.Color;
        // indicator.transform.SetParent(nextActionsPanel);
    }

    internal void RemoveUnit(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.Q<UnitItem>().Unit == unit).ForEach(x => x.RemoveFromHierarchy());
    }

    internal void OnTurnStart(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.SetEnabled(true));
    }

    internal void OnTurnEnd(Unit unit)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.UnitItem.Unit == unit).ForEach(x => x.SetEnabled(false));
    }
}
