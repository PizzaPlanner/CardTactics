using UnityEngine;

[RequireComponent(typeof(GridPosition))]
public class TriggerComponent : MonoBehaviour
{
    public Trigger Trigger;
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(Trigger.gameObject.name));
        UnityEditor.Handles.Label(transform.position + Vector3.up, Trigger.gameObject.name);
    }
#endif
}
