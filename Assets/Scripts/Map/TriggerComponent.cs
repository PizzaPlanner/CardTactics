using UnityEngine;

[RequireComponent(typeof(GridPosition))]
public class TriggerComponent : MonoBehaviour
{
    public Trigger Trigger;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var origin = transform.position + Vector3.up * 0.5f;
        UnityEditor.Handles.DrawLine(origin + new Vector3(0, 0, -0.4f), origin + new Vector3(0, 0, 0.1f), 8);
        UnityEditor.Handles.DrawLine(origin + new Vector3(0, 0, 0.2f), origin + new Vector3(0, 0, 0.25f), 8);
    }
#endif
}
