using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GridPosition))]
public class Owner : MonoBehaviour
{
    public int Id;

#if UNITY_EDITOR
    Map map;
    void OnDrawGizmosSelected()
    {
        if (map == null) map = GetComponentInParent<Map>();
        if (Id < map.AllTeams.Count)
            UnityEditor.Handles.color = map.AllTeams[Id].Color;
        else
            UnityEditor.Handles.color = Color.black;

        var origin = transform.position + Vector3.up * 0.5f;

        UnityEditor.Handles.DrawLine(origin + new Vector3(0.5f, 0, 0.5f) * 0.7f, origin + new Vector3(0.5f, 0, -0.5f) * 0.7f, 3);
        UnityEditor.Handles.DrawLine(origin + new Vector3(0.5f, 0, -0.5f) * 0.7f, origin + new Vector3(-0.5f, 0, -0.5f) * 0.7f, 3);
        UnityEditor.Handles.DrawLine(origin + new Vector3(-0.5f, 0, -0.5f) * 0.7f, origin + new Vector3(-0.5f, 0, 0.5f) * 0.7f, 3);
        UnityEditor.Handles.DrawLine(origin + new Vector3(-0.5f, 0, 0.5f) * 0.7f, origin + new Vector3(0.5f, 0, 0.5f) * 0.7f, 3);
    }
#endif
}
