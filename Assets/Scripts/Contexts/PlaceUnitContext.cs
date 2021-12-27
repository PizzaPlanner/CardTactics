using System.Collections;
using Mirror;
using UnityEngine;

public struct PlaceUnitContext : IPlayableContext, IClientPlayable, IServerPlayable
{
    public Unit Unit;
    public Tile Target, Source;
    public Facing Facing;

    public bool Queueable => true;

    public string Name => "Place " + Unit.Name + " at " + Target.Position;

    public IEnumerator PlayOnClient()
    {
        if (!Target.Place(Unit, Facing, true))
            Debug.LogError("Failed to place Unit");
        else if (Source != null)
            Source.Unit = null;
        yield return null;
    }

    public void PlayOnServer()
    {
        if (!Target.Place(Unit, Facing, false))
            Debug.LogError("Failed to place Unit");
        else if (Source != null)
            Source.Unit = null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteNetworkBehaviour(Unit);
        writer.WriteTile(Target);
        writer.WriteTile(Source);
        writer.WriteByte((byte)Facing);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Unit = reader.ReadNetworkBehaviour<Unit>();
        Target = reader.ReadTile();
        Source = reader.ReadTile();
        Facing = (Facing)reader.ReadByte();
        return this;
    }
}
