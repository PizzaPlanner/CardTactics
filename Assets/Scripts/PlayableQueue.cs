using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlayableQueue
{
    const bool DO_LOG = true;
    public NetworkManagerMode Mode { get; private set; }
    public PlayableQueue(NetworkManagerMode mode)
    {
        Mode = mode;
        queue = new();
    }

    public bool Empty => queue.Count == 0;

    readonly Queue<IPlayableContext> queue;

    public void Enqueue(IPlayableContext context) => Enqueue(context, false);
    public void InsertFirst(IPlayableContext context) => Enqueue(context, true);

    public IPlayableContext Next() => queue.Dequeue();

    public IPlayableContext Current { get; private set; }

    void Enqueue(IPlayableContext context, bool priorizeNext)
    {
        if (DO_LOG)
            PrintLogs(context);

        if (priorizeNext)
        {
            var old = queue.ToArray();
            queue.Clear();
            queue.Enqueue(context);
            foreach (var item in old)
                queue.Enqueue(item);
        }
        else
            queue.Enqueue(context);
    }

    private void PrintLogs(IPlayableContext context)
    {
        var mode = Mode switch
        {
            NetworkManagerMode.Offline => "Offline",
            NetworkManagerMode.ServerOnly => "Host",
            NetworkManagerMode.ClientOnly => "Client",
            NetworkManagerMode.Host => "Host",
            _ => "?"
        };

        if (!context.Queueable)
        {
            if (context is IPlayableContextWithUnit ipcu && ipcu.Unit != null)
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} for {ipcu.Unit.Name} on {mode}", ipcu.Unit);
            else
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} on {mode}");
            return;
        }
        if (context is IPlayableContextWithUnit ipcu2 && ipcu2.Unit != null)
            Debug.Log($"[Playable] Queue {context.Name} for {ipcu2.Unit.Name} on {mode}", ipcu2.Unit);
        else
            Debug.Log($"[Playable] Queue {context.Name} on {mode}");
    }

    public bool TryGetNext(out IEnumerator coroutine)
    {
        coroutine = null;
        if (Current != null || Empty) return false;

        Current = Next();

        if (DO_LOG)
        {
            var mode = Mode switch
            {
                NetworkManagerMode.Offline => "Offline",
                NetworkManagerMode.ServerOnly => "Host",
                NetworkManagerMode.ClientOnly => "Client",
                NetworkManagerMode.Host => "Host",
                _ => "?"
            };
            if (Current is IPlayableContextWithUnit ipcu && ipcu.Unit != null)
                Debug.Log($"[Playable] [{mode}] Play {Current.Name} for {ipcu.Unit.Name}", ipcu.Unit);
            else
                Debug.Log($"[Playable] [{mode}] Play {Current.Name}");
        }
        coroutine = PlayAndResume();
        return true;
    }

    IEnumerator PlayAndResume()
    {
        if (Mode == NetworkManagerMode.ClientOnly)
        {
            if (Current is IClientPlayable c)
                yield return c.PlayOnClient();
        }
        else
        {
            bool sendToClient = Current is IClientPlayable;
            if (Current is IServerWaitPlayable sw)
                yield return sw.PlayOnServer();
            else if (Current is IServerPlayable s)
                sendToClient &= s.PlayOnServer();

            if (sendToClient)
            {
                var c = Current as IClientPlayable;
                if (!c.ManualSending)
                    NetworkServer.SendToAll(new PlayableMessage { context = Current });
                if (Mode == NetworkManagerMode.Host && Current is not IServerWaitPlayable)
                    yield return c.PlayOnClient();
            }
        }
        Current = null;
    }
}
