using UnityEngine;

public class GridPosition : MonoBehaviour
{
    public Vector3Int Position;
    public int Height;

    const float Y_PER_HEIGHT = 0.25f;
    static readonly Vector3 offset = Vector3.one * 0.5f + Vector3.down;

    void Start()
    {
        SetPosition();
    }

    public void SetPosition()
    {
        var parent = GetComponentInParent<Grid>();
        transform.localPosition = parent.CellToLocal(Position) + offset + Vector3.up * Height * Y_PER_HEIGHT;
    }

    void OnDrawGizmosSelected()
    {
        SetPosition();
    }
}
