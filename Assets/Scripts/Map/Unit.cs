using System;
using System.Collections;
using UnityEngine;
public class Unit : MonoBehaviour
{
    public PlayerInfo Player { get => player; set { player = value; /*GetComponent<Outline>().OutlineColor = value.Color;*/ } }
    PlayerInfo player;
    public int Level;
    public int Rank;
    public string Name;
    public Sprite Icon;
    public int Speed;

    public UnitCard Card;

    public float MaxLife;
    public float Life { get; private set; }
    public float LifePreview { get; private set; }

    public UnitFilter Type;

    public void Damage(float dmg, bool isPreview = false)
    {
        if (!isPreview)
        {
            Life -= dmg;
            if (Life > MaxLife)
                Life = MaxLife;
            LifePreview = Life;
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

    void Start()
    {
        Life = MaxLife;
        LifePreview = MaxLife;
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
            foreach (var tile in to.Pathing.LastCalculatedPath)
            {
                transform.position = tile.transform.position + Vector3.up * 0.5f;
                transform.rotation = Quaternion.Euler(0, tile.FaceToRot(tile.GetPathFacing()), 0);
                yield return new WaitForSeconds(0.1f);
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
