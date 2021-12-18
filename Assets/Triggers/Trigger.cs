using System;
using System.Collections;
using Mirror;
using UnityEngine;

public interface IPlayable
{
}

public class TriggerContext : IPlayableContextWithUnit, IServerPlayable
{
    public Trigger Target { get; private set; }
    public Unit Unit { get; private set; }

    public TriggerContext(Unit unit, Trigger trigger)
    {
        Target = trigger;
        Unit = unit;
    }

    public string Name => Target.GetType().Name;
    public bool Queueable => Unit != null && (Target.CanTriggerMultipleTimes || !Target.Executed);

    public IEnumerator PlayOnServer()
    {
        if (Target.Executed && !Target.CanTriggerMultipleTimes)
            yield break;
        Target.Executed = true;
        yield return Target.Execute(Unit);
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }
}

[System.Serializable]
public abstract class Trigger : MonoBehaviour, IPlayable
{
    [SerializeField] public bool CanTriggerMultipleTimes;

    [NonSerialized] public bool Executed;

    public abstract IEnumerator Execute(Unit unit);
}