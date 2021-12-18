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

    [SerializeField] Deck deck;

    public bool IsAI => slot == SlotType.AI;

    public SlotType Slot => slot;
    [SerializeField] SlotType slot;

    public Trigger OnDeadSelf, OnDeadKiller;

    public bool GetUnit(Team team, Map map, out Unit unit)
    {
        unit = null;
        var allPlayers = FindObjectsOfType<PlayerController>().ToList();
        allPlayers.Sort((x, y) => x.ControlledUnits.Count.CompareTo(y.ControlledUnits.Count));
        var player = slot == SlotType.AI ? null : allPlayers.FirstOrDefault();
        if (slot != SlotType.AI && player == null)
        {
            Debug.LogError("Incompatible Player Slot assignment");
            return false;
        }

        if (deck == null && (player == null || player.Deck == null)) return false;
        if (deck == null)
            unit = player.Deck.Create();
        else
            unit = deck.Create();
        var teamId = -1;
        for (int t = 0; t < map.Teams.Length; t++)
            if (map.Teams[t] == team)
                teamId = t;
        unit.Player = new PlayerInfo { IsAI = IsAI, TeamColor = team.Color, Team = teamId };
        unit.Owner = player;
        unit.OnDeadSelf = OnDeadSelf;
        unit.OnDeadKiller = OnDeadKiller;
        unit.Life = unit.MaxLife; // We have to set it here to not immediately trigger win conditions
        if (player != null)
        {
            player.ControlledUnits.Add(unit);
            Debug.Log($"Assigned {unit.Name} to {player.gameObject.name}");
        }
        unit.gameObject.SetActive(false);
        return true;
    }
}