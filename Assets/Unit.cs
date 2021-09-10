using UnityEngine;
public class Unit : MonoBehaviour
{
    public int Player { get => player; set { player = value; GetComponent<Outline>().OutlineColor = Board.PlayerColor(value); } }
    int player;
    public int Level;
    public int Rank;
    public string Name;
    public Texture2D Icon;

    public float MaxLife;
    public float Life { get; private set; }
    public float LifePreview { get; private set; }

    public void Damage(float dmg, bool isPreview = false)
    {
        if (isPreview)
            LifePreview -= dmg;
        else
            Life -= dmg;
    }

    void Start()
    {
        Life = MaxLife;
        LifePreview = MaxLife;
    }

    public void Move(Tile from, Tile to)
    {
        if (from.Unit != this || to.Unit != null)
            return;
        from.Unit = null;
        var facing = to.GetPathFacing();
        if (facing == Facing.Preserve)
            facing = Tile.GetFacing(from, to);
        to.Place(this, facing);
    }
}
