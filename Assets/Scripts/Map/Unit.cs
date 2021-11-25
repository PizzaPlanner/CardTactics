using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

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

public class Unit : MonoBehaviour
{
    public const float MOVEMENT_SPEED = 8f;

    public string Name;
    public Sprite Icon;
    public int Speed;
    public float MaxLife;
    public UnitFilter Type;
    public ResourceInfo Resources;

    public float Life { get; private set; }
    public float LifePreview { get; private set; }
    public PlayerInfo Player { get; set; }

    static int spawnCounter;
    int counter;

    public void Damage(float dmg, bool isPreview = false)
    {
        if (!isPreview)
        {
            Life -= dmg;
            if (Life > MaxLife)
                Life = MaxLife;
            LifePreview = Life;
            OnLifeChange?.Invoke(Life, MaxLife);
            if (Life <= 0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            LifePreview -= dmg;
            if (LifePreview > MaxLife)
                LifePreview = MaxLife;
        }
    }

    public Action<float, float> OnLifeChange;

    void OnDestroy()
    {
        if (Player != null)
            FindObjectOfType<TurnPanel>()?.RemoveUnit(this);
    }

    void Start()
    {
        Life = MaxLife;
        LifePreview = MaxLife;
        counter = spawnCounter++;
        if (Player != null)
            FindObjectOfType<TurnPanel>()?.AddUnit(this);
        OnLifeChange?.Invoke(Life, MaxLife);
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
        if (from.Unit != this || to.Unit != null)
            return;
        from.Unit = null;
        var facing = to.GetPathFacing();
        if (facing == Facing.Preserve)
            facing = Tile.GetFacing(from, to);
        to.Place(this, facing);
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
