using UnityEngine;

[RequireComponent(typeof(GridPosition))]
public class SpawnLocation : MonoBehaviour
{
    public Facing Facing;
    public bool InstantCommanderSpawn;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = InstantCommanderSpawn ? Color.cyan : Color.white;

        var origin = transform.position + Vector3.up * 0.5f;

        switch (Facing)
        {
            case Facing.Left:
                UnityEditor.Handles.DrawLine(origin + new Vector3(0.1f, 0, 0.2f), origin + new Vector3(-0.1f, 0, 0), 3);
                UnityEditor.Handles.DrawLine(origin + new Vector3(0.1f, 0, -0.2f), origin + new Vector3(-0.1f, 0, 0), 3);
                break;
            case Facing.Right:
                UnityEditor.Handles.DrawLine(origin + new Vector3(-0.1f, 0, 0.2f), origin + new Vector3(0.1f, 0, 0), 3);
                UnityEditor.Handles.DrawLine(origin + new Vector3(-0.1f, 0, -0.2f), origin + new Vector3(0.1f, 0, 0), 3);
                break;
            case Facing.Up:
                UnityEditor.Handles.DrawLine(origin + new Vector3(-0.2f, 0, -0.1f), origin + new Vector3(0, 0, 0.1f), 3);
                UnityEditor.Handles.DrawLine(origin + new Vector3(0.2f, 0, -0.1f), origin + new Vector3(0, 0, 0.1f), 3);
                break;
            case Facing.Down:
                UnityEditor.Handles.DrawLine(origin + new Vector3(-0.2f, 0, 0.1f), origin + new Vector3(0, 0, -0.1f), 3);
                UnityEditor.Handles.DrawLine(origin + new Vector3(0.2f, 0, 0.1f), origin + new Vector3(0, 0, -0.1f), 3);
                break;
        }
    }
#endif
}
