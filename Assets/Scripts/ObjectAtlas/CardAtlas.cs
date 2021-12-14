using Mirror;
using UnityEngine;

public class CardAtlas : ObjectAtlas<Card>
{
#if UNITY_EDITOR
    protected override string[] References(string[] guids) => guids;
#endif
}