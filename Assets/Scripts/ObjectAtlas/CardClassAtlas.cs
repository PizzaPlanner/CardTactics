using Mirror;
using UnityEngine;

public class CardClassAtlas : ObjectAtlas<CardClass>
{
#if UNITY_EDITOR
    protected override string[] References(string[] guids) => guids;
#endif
}