using Mirror;
using UnityEngine;

public class Actor : NetworkBehaviour
{
    [SyncVar] public int Speed;
    [SyncVar] int counter;

    static int Counter;


    public override void OnStartServer()
    {
        counter = Counter++;
    }

    public static int CompareSpeed(Actor u1, Actor u2)
    {
        var x = u1.Speed.CompareTo(u2.Speed);
        if (x == 0)
            return u1.counter.CompareTo(u2.counter);
        else
            return x;
    }
}