using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ResourceInfo
{
    public int Energy;
    public int EnergySpent { get; set; }
    public CardClass[] Classes;

    public bool CanAfford(ResourceInfo other)
    {
        var classes = Classes;
        return Energy - EnergySpent >= other.Energy && other.Classes.Count(x => !classes.Contains(x)) == 0;
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
    public UnitCard Card { get; set; }

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
                Card?.Destroy();
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

    public IEnumerator Move(Tile from, Tile to, RangeFilter type)
    {
        if (from.Unit != this || to.Unit != null)
            yield break;
        if (type == RangeFilter.Pathfinding)
        {
            if (Tile.Distance(from, to) > 1)
                for (int i = 1; i < to.Pathing.LastCalculatedPath.Count; i++)
                {
                    var f = to.Pathing.LastCalculatedPath[i - 1];
                    var t = to.Pathing.LastCalculatedPath[i];
                    if (Tile.Distance(f, t) == 1)
                        yield return Move(f, t, type);
                    else
                        yield return Move(f, t, RangeFilter.Any);
                }
            else
            {
                transform.rotation = Quaternion.Euler(0, to.FaceToRot(to.GetPathFacing()), 0);
                var target = to.UnitAnchor;
                while (Vector3.Distance(transform.position, target) > MOVEMENT_SPEED * Time.deltaTime)
                {
                    transform.position += (target - transform.position).normalized * MOVEMENT_SPEED * Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
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
        MoveInstant(from, to);
        yield return new WaitForEndOfFrame();
    }
}
