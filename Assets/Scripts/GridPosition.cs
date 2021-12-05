using UnityEngine;

public class GridPosition : MonoBehaviour
{
    public Vector3Int Position;

    const float Y_PER_HEIGHT = 0.25f;
    static readonly Vector3 offset = new Vector3(0.5f, 0, 0.5f);

    void Start()
    {
        SetPosition();
    }

    public void SetPosition()
    {
        var parent = GetComponentInParent<Grid>();
        transform.localPosition = parent.CellToLocal(Position) + offset;
    }

    void OnDrawGizmosSelected()
    {
        SetPosition();
    }
}
