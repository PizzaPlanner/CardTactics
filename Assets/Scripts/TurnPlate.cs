using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnPlate : MonoBehaviour
{
    [SerializeField] VisualTreeAsset readyBadgeAsset;
    VisualElement readyPanel;
    [SerializeField] RectTransform nextActionsPanel;

    [SerializeField] CardDisplay turnIndicatorPrefab;

    void Awake()
    {
        readyPanel = FindObjectOfType<UIDocument>().rootVisualElement.Q("ReadyPanel");
        Debug.Assert(readyPanel != null, this);
        Hide();

        gameObject.SetActive(false);
        foreach (Transform t in nextActionsPanel)
            Destroy(t.gameObject);
    }

    void Hide()
    {
        readyPanel.style.display = DisplayStyle.None;
    }

    public void AddPlayer(PlayerInfo player)
    {
        readyBadgeAsset.CloneTree(readyPanel, out int idx, out int _);
        var badge = readyPanel[idx].Q<PlayerInfoBadge>();
        badge.Player = player;
        badge.Q<CardItemSmall>().Card = player.Deck.Commander;
        Debug.Log($"New Player: {player.Name}");
    }

    public void OnRoundStart()
    {
        readyPanel.style.display = DisplayStyle.None;
        gameObject.SetActive(true);
    }

    public void OnPreparation()
    {
        gameObject.SetActive(false);
        readyPanel.style.display = DisplayStyle.Flex;
        readyPanel.Query<PlayerInfoBadge>()
        .ForEach(x => x.Q<ResourcePanel>().Deck = x.Player.Deck);
        foreach (Transform t in nextActionsPanel)
            Destroy(t.gameObject);
    }

    public void OnPlayerReady(PlayerInfo player)
    {

    }

    public void OnCardInQueue(Card card, PlayerInfo player)
    {
        var indicator = Instantiate(turnIndicatorPrefab);
        indicator.GetComponent<SkillProgressBar>().SetSkills(card, player);
        indicator.SetCard(card);
        indicator.GetComponentInChildren<UnitDisplay>().GetComponent<CardDisplay>().SetCard(player.Deck.Commander);
        // indicator.PlayerColorBorder.color = player.Color;
        indicator.transform.SetParent(nextActionsPanel);
    }

    internal void RemovePlayer(PlayerInfo player)
    {
        readyPanel.Query<PlayerInfoBadge>().Where(x => x.Player == player).ForEach(x => x.RemoveFromHierarchy());
        Debug.Log($"{player.Name} died!");
    }
}
