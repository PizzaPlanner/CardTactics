using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(Map))]
public class MapEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();
    }
}