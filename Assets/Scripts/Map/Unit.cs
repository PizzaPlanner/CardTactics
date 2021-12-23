using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

public enum ResourceState
{
    Available,
    Used,
    Destroyed
}

[System.Serializable]
public struct Resource
{
    public int Total;
    public CardClass Type;
    public int Available => Total - Used;
    [NonSerialized] public int Used;
}

[System.Serializable]
public class ResourceInfo
{
    [TableList(AlwaysExpanded = true)]
    public List<Resource> Resources;

    public bool CanAfford(ResourceInfo other)
    {
        foreach (var res in other.Resources)
            if (!Resources.Any(x => x.Type == res.Type && x.Available >= res.Available)) return false;
        return true;
    }

    public void Spend(ResourceInfo other)
    {
        foreach (var otherRes in other.Resources)
            for (int i = 0; i < Resources.Count; i++)
                if (Resources[i].Type == otherRes.Type)
                    Resources[i] = new Resource
                    {
                        Type = Resources[i].Type,
                        Total = Resources[i].Total,
                        Used = Resources[i].Used + otherRes.Available,
                        // NonRefreshable = Resources[i].NonRefreshable
                    };
    }

    public void Refresh()
    {
        for (int i = 0; i < Resources.Count; i++)
            Resources[i] = new Resource
            {
                Type = Resources[i].Type,
                Total = Resources[i].Total,
                Used = 0,
            };
    }

    public void Add(CardClass type, int amount)
    {
        Debug.Log($"Add {amount} {type.Name}");
        bool found = false;
        for (int i = 0; i < Resources.Count; i++)
            if (Resources[i].Type == type)
            {
                var r2 = Resources[i];
                r2.Total += amount;
                Resources[i] = r2;
                found = true;
                break;
            }
        if (!found)
            Resources.Add(new Resource { Type = type, Total = amount });
    }

    public void Remove(CardClass type, int amount)
    {
        for (int i = 0; i < Resources.Count; i++)
            if (Resources[i].Type == type && amount > 0)
            {
                var r2 = Resources[i];
                r2.Total -= amount;
                r2.Used = Mathf.Max(0, r2.Used - amount);
                amount = -r2.Total;
                Resources[i] = r2;
                if (r2.Total <= 0)
                {
                    Resources.Remove(r2);
                    i--;
                }
            }
    }
}

public class Unit : NetworkBehaviour
{
    public const float MOVEMENT_SPEED = 8f;

    public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Player.TeamColor)}>{name}</color>";
    public Sprite Icon;
    [SyncVar] public int Speed;
    [SyncVar] public float MaxLife;
    public UnitFilter Type;
    public ResourceInfo Resources;

    public PlayerController Owner;

    public bool IsDead => Life <= 0 || Player.Team == -1;
    [SyncVar] public float Life;
    [SyncVar] public PlayerInfo Player;

    static int spawnCounter;
    [SyncVar] int counter;

    public Trigger OnDeadSelf, OnDeadKiller;

    public void Damage(float dmg, Unit source)
    {

        Life -= dmg;
        if (Life > MaxLife)
            Life = MaxLife;
        if (IsDead)
        {
            OnDeath(source);
        }
    }

    void OnDeath(Unit killer)
    {
        Debug.Log($"[Unit] {name} died!");
        gameObject.SetActive(false);
        if (Player.Team != -1)
        {
            FindObjectOfType<TurnPanel>()?.RemoveUnit(this);
            if (OnDeadKiller != null && killer != null)
                RoomNetworkManager.Playables.InsertFirst(new TriggerContext(killer, OnDeadKiller));
            if (OnDeadSelf != null)
                RoomNetworkManager.Playables.InsertFirst(new TriggerContext(this, OnDeadSelf));
        }
        if (bar != null)
            bar.RemoveFromHierarchy();
    }

    public override void OnStartServer()
    {
        counter = spawnCounter++;
    }

    public override void OnStartClient()
    {
        if (Player.Team != -1)
        {
            FindObjectOfType<TurnPanel>()?.AddUnit(this);
            foreach (var mr in gameObject.GetComponentsInChildren<Renderer>())
                mr.gameObject.layer = LayerMask.NameToLayer("Outline");
        }
        if (MaxLife > 0)
        {
            bar = FindObjectOfType<TurnPanel>()?.MakeStatusBar();
            bar.style.width = (MaxLife / 5) * 25 + 80;
            bar.Unit = this;
            bar.FollowUnit = true;
        }
        Debug.Log("Started on Client: " + Name);
    }

    internal void Spawn()
    {
        gameObject.SetActive(true);
        NetworkServer.Spawn(gameObject, Owner?.connectionToClient);
        Debug.Log("Spawning " + Name);
    }

    UnitStatusBar bar;
    void LateUpdate()
    {
        if (bar != null)
            bar.UpdateValues();
    }

    public static int CompareSpeed(Unit u1, Unit u2)
    {
        var x = u1.Speed.CompareTo(u2.Speed);
        if (x == 0)
            return u1.counter.CompareTo(u2.counter);
        else
            return x;
    }

    public void MoveInstant(Tile from, Tile to)
    {
        if (from.Unit != this)
            return;
        var facing = to.GetPathFacing();
        if (facing == Facing.Preserve)
            facing = Tile.GetFacing(from, to);
        if (to.Place(this, facing))
            from.Unit = null;
    }

    public IEnumerator Move(Tile source, ITilePattern pattern, Tile target, RangeFilter type)
    {

        var tiles = pattern.Tiles.ToList();
        if (!tiles.Contains(source))
            tiles.Insert(0, source);
        if (target != null)
        {
            // Make sure target is always last in the list
            if (tiles.Contains(target))
                tiles.Remove(target);
            tiles.Add(target);
        }

        var current = 0;
        while (tiles.Count > current + 1)
        {
            var from = tiles[current];
            var to = tiles[++current];

            if (GameSettings.Instance.UseAnimations)
            {
                if (type == RangeFilter.Path)
                {
                    transform.rotation = Quaternion.Euler(0, to.FaceToRot(Tile.GetFacing(from, to)), 0);
                    while (Vector3.Distance(transform.position, to.UnitAnchor) > MOVEMENT_SPEED * Time.deltaTime)
                    {
                        transform.position += (to.UnitAnchor - transform.position).normalized * MOVEMENT_SPEED * Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                }
                else if (type == RangeFilter.Any)
                {
                    while (transform.position.y < 5)
                    {
                        transform.position += Vector3.up * 50 * Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    transform.position = to.transform.position + Vector3.up * 5;
                    transform.rotation = Quaternion.Euler(0, to.FaceToRot(Tile.GetFacing(from, to)), 0);
                    while (transform.position.y > 0)
                    {
                        transform.position -= Vector3.up * 50 * Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            MoveInstant(from, to);
            yield return new WaitForEndOfFrame();
        }
    }
}
