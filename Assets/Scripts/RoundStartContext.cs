using System.Collections;
using Mirror;

public class RoundStartContext : IPlayableContext, IClientPlayable
{
    public bool Queueable => true;
    public string Name => $"Round {Round}";
    public int Round { get; set; }

    public IEnumerator PlayOnClient()
    {
        yield return TurnPanel.FindObjectOfType<TurnPanel>().OnRoundStart(Round, onComplete: null);
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteInt(Round);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Round = reader.ReadInt();
        return this;
    }
}
