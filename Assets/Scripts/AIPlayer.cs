using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer
{
    public PlayerSlot Player;
    public Card[] PlanTurn(Map map)
    {
        var selfTiles = map.GetPlayerTiles(Player);
        var enemyTiles = new List<Tile>();
        foreach (var enemy in map.Players.Where(x => x.Team != Player.Team))
            enemyTiles.AddRange(map.GetPlayerTiles(enemy));

        return new Card[0];
    }
}