using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnPlate : MonoBehaviour
{
    [SerializeField] RectTransform readyPanel;
    [SerializeField] RectTransform nextActionsPanel;

    [SerializeField] PlayerInfoPanel playerInfoPanelPrefab;

    [SerializeField] CardDisplay turnIndicatorPrefab;

    Dictionary<PlayerInfo, PlayerInfoPanel> badges;

    void Awake()
    {
        gameObject.SetActive(false);
        badges = new Dictionary<PlayerInfo, PlayerInfoPanel>();
        foreach (Transform t in readyPanel)
            Destroy(t.gameObject);
    }

    public void AddPlayer(PlayerInfo player)
    {
        var badge = GameObject.Instantiate(playerInfoPanelPrefab);
        badge.Player = player;
        badge.transform.SetParent(readyPanel);
        badges[player] = badge;
    }

    public void OnRoundStart()
    {
        readyPanel.gameObject.SetActive(false);
        gameObject.SetActive(true);
        foreach (Transform t in nextActionsPanel)
            Destroy(t.gameObject);
    }

    public void OnPreparation()
    {
        gameObject.SetActive(false);
        readyPanel.gameObject.SetActive(true);
        foreach (var kvp in badges)
        {
            kvp.Value.gameObject.SetActive(true);
            kvp.Value.IsReady = false;
            kvp.Value.UpdateValues();
        }
    }

    public void OnPlayerReady(PlayerInfo player)
    {
        badges[player].GetComponentInChildren<CardDisplay>().Speed.text = $"<color=green>{player.Deck.CurrentSpeed.ToString()}</color>";
    }

    public void OnCardInQueue(Card card, PlayerInfo player)
    {
        var indicator = Instantiate(turnIndicatorPrefab);
        indicator.GetComponent<SkillProgressBar>().SetSkills(card, player);
        indicator.SetCard(card);
        indicator.PlayerColorBorder.color = player.Color;
        indicator.transform.SetParent(nextActionsPanel);
    }

    internal void RemovePlayer(PlayerInfo player)
    {
        Destroy(badges[player]);
        badges.Remove(player);
    }
}
