using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[System.Serializable]
public struct PlayerInfo
{
    public bool IsAI;
    public Color TeamColor;
    public int Team;
}


[System.Serializable]
public class PlayerSlot : MonoBehaviour
{
    public enum SlotType
    {
        PlayerRequired,
        PlayerOptional,
        AI
    }

    [ShowIf("IsAI")]
    [SerializeField] Deck deck;
    [ShowIf("IsAI")]
    [SerializeField] Unit Unit;

    public bool IsAI => slot == SlotType.AI;

    public SlotType Slot => slot;
    [SerializeField] SlotType slot;

    public Trigger OnDeadSelf, OnDeadKiller;

    public static List<int> decksAssigned;

    public bool GetUnit(Team team, Map map, out Unit unit)
    {
        unit = null;
        var player = new JourneyData.PlayerWithDeck { PlayerId = -1 };
        if (slot != SlotType.AI)
        {
            for (int i = 0; i < JourneyData.Current.SelectedDecks.Count; i++)
                if (!decksAssigned.Contains(i))
                {
                    decksAssigned.Add(i);
                    player = JourneyData.Current.SelectedDecks[i];
                    Debug.Log("Assigned Deck " + i + " to " + slot);
                    break;
                }

            if (player.PlayerId == -1)
            {
                if (slot == SlotType.PlayerRequired)
                    Debug.LogError("No Deck assigned for Required Player");
                else
                    Debug.Log("No Deck assigned for Optional Player");
                return false;
            }
        }
        else if (deck == null && Unit == null)
        {
            Debug.LogError("No Deck assigned for AI Player");
            return false;
        }

        if (Unit != null)
            unit = Instantiate(Unit);
        else if (deck == null)
            unit = JourneyData.Current.AvailableDecks[player.DeckId].Create();
        else
            unit = deck.Create();
        var teamId = -1;
        for (int t = 0; t < map.Teams.Length; t++)
            if (map.Teams[t] == team)
                teamId = t;
        unit.Player = new PlayerInfo { IsAI = IsAI, TeamColor = team.Color, Team = teamId };
        unit.Owner = player.PlayerId >= 0 ? PlayerController.FromLobby(player.PlayerId) : null;
        unit.OnDeadSelf = OnDeadSelf;
        unit.OnDeadKiller = OnDeadKiller;
        unit.gameObject.SetActive(false);
        return true;
    }
}