using Mirror;
using UnityEngine;

public class CardAtlas : ObjectAtlas<Card>
{
    protected override string[] References(string[] guids) => guids;
}