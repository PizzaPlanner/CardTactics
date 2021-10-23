using UnityEngine;

[RequireComponent(typeof(GridPosition))]
public class Marker : MonoBehaviour
{
    public int Id;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.HSVToRGB(Id * 7 / 40f, .9f, 0.8f);
        var origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origin + new Vector3(0.5f, 0, 0.5f) * 0.5f, origin + new Vector3(0.5f, 0, -0.5f) * 0.5f);
        Gizmos.DrawLine(origin + new Vector3(0.5f, 0, -0.5f) * 0.5f, origin + new Vector3(-0.5f, 0, -0.5f) * 0.5f);
        Gizmos.DrawLine(origin + new Vector3(-0.5f, 0, -0.5f) * 0.5f, origin + new Vector3(-0.5f, 0, 0.5f) * 0.5f);
        Gizmos.DrawLine(origin + new Vector3(-0.5f, 0, 0.5f) * 0.5f, origin + new Vector3(0.5f, 0, 0.5f) * 0.5f);
    }
#endif
}
