using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public const float MOVEMENT_SPEED = 8f;

    public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Player.TeamColor)}>{name}</color>";
    public Sprite Icon;
    [SyncVar] public int Speed;
    [SyncVar] public float MaxLife;
    [SyncVar] public ResourceInfo Resources;
    public UnitFilter Type;

    public PlayerController Owner;

    public bool IsDead => Life <= 0 || Player.Team == -1;
    float Life => MaxLife - MissingLife;

    [SyncVar] float MissingLife;
    [SyncVar] public PlayerInfo Player;

    public float DisplayLife;
    public bool UpdateDisplayLife { set; private get; }

    static int spawnCounter;
    [SyncVar] int counter;

    public Trigger OnDeadSelf, OnDeadKiller;

    public void Damage(float dmg, Unit source)
    {

        MissingLife += dmg;
        if (MissingLife > MaxLife)
            MissingLife = MaxLife;
        if (IsDead)
        {
            OnDeath(source);
        }
    }

    void OnDeath(Unit killer)
    {
        Debug.Log($"[Unit] {name} died!");
        // gameObject.SetActive(false);
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
        NetworkServer.UnSpawn(gameObject);
    }

    public override void OnStartServer()
    {
        counter = spawnCounter++;
    }

    public override void OnStartClient()
    {
        UpdateDisplayLife = true;

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

    [Server]
    internal void Spawn()
    {
        transform.position = new Vector3(-9999, -9999, -9999);
        gameObject.SetActive(true);
        NetworkServer.Spawn(gameObject, Owner?.connectionToClient);
        Debug.Log("Spawning " + Name);
    }

    UnitStatusBar bar;
    void LateUpdate()
    {
        if (UpdateDisplayLife)
        {
            if (DisplayLife < Life)
                DisplayLife += Mathf.Min(Time.deltaTime * (MaxLife - MissingLife * 0.5f), Life - DisplayLife);
            else if (DisplayLife > Life)
                DisplayLife -= Mathf.Min(Time.deltaTime * (MaxLife - MissingLife * 0.5f), DisplayLife - Life);
            else
                UpdateDisplayLife = false;
        }
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
        if (isServer)
            RoomNetworkManager.Playables.InsertFirst(new PlaceUnitContext { Unit = this, Source = from, Target = to, Facing = facing });
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
            transform.position = to.UnitAnchor;
            yield return new WaitForEndOfFrame();
        }
    }
}
