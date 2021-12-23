using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayableQueue
{
    bool isClient;
    public PlayableQueue(bool client)
    {
        isClient = client;
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
        if (!context.Queueable)
        {
            if (context is IPlayableContextWithUnit ipcu && ipcu.Unit != null)
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} for {ipcu.Unit.Name} on {(isClient ? "Client" : "Server")}", ipcu.Unit);
            else
                Debug.LogError($"[Playable] Cannot queue unqueueable {context.Name} on {(isClient ? "Client" : "Server")}");
            return;
        }
        if (context is IPlayableContextWithUnit ipcu2 && ipcu2.Unit != null)
            Debug.Log($"[Playable] Queue {context.Name} for {ipcu2.Unit.Name} on {(isClient ? "Client" : "Server")}", ipcu2.Unit);
        else
            Debug.Log($"[Playable] Queue {context.Name} on {(isClient ? "Client" : "Server")}");

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

    public bool TryGetNext(out IEnumerator coroutine)
    {
        coroutine = null;
        if (Current != null || Empty) return false;

        Current = Next();

        if (Current is IPlayableContextWithUnit ipcu && ipcu.Unit != null)
            Debug.Log($"[Playable] [{(isClient ? "Client" : "Server")}] Play {Current.Name} for {ipcu.Unit.Name}", ipcu.Unit);
        else
            Debug.Log($"[Playable] [{(isClient ? "Client" : "Server")}] Play {Current.Name}");
        coroutine = PlayAndResume();
        return true;
    }

    IEnumerator PlayAndResume()
    {
        if (isClient)
        {
            if (Current is IClientPlayable c)
                yield return c.PlayOnClient();
        }
        else
        {
            if (Current is IServerPlayable s)
                yield return s.PlayOnServer();
            if (Current is IClientPlayable c && !c.ManualSending)
                NetworkServer.SendToAll(new PlayableMessage { context = Current });
        }
        Current = null;
    }
}
