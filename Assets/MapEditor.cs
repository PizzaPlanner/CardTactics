#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public partial class Map : SerializedScriptableObject
{
    enum MapEditMode
    {
        Tile,
        Area,
        Trigger,
        Height,
        Spawn
    }

    [EnumToggleButtons]
    [ShowInInspector]
    static MapEditMode editMode;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Tile)]
    [AssetList]
    static Tile drawSelected;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Area)]
    [Range(0, 25)]
    static int drawArea;

    [ShowInInspector]
    [ShowIf("editMode", MapEditMode.Trigger)]
    [Range(1, 25)]
    static int drawTrigger;

    Trigger DrawTriggers(Trigger value)
    {
        var x = -1;
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < Triggers.Length; i++)
        {
            if (Triggers[i] == value)
            {
                if (x != -1)
                {
                    Debug.LogError("The same trigger is referenced multiple Times!");
                }
                else
                {
                    x = i;
                }
            }
        }
        EditorGUILayout.LabelField((x + 1).ToString(), GUILayout.Width(25));

        var trigger = (Trigger)EditorGUILayout.ObjectField(value, typeof(Trigger), false);
        EditorGUILayout.EndHorizontal();
        return trigger;
    }

    static TilePrefabWithInfo DrawCell(Rect rect, TilePrefabWithInfo value)
    {
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && rect.Contains(Event.current.mousePosition))
        {
            if (value == null)
                value = new TilePrefabWithInfo();
            if (editMode == MapEditMode.Tile)
                value.Prefab = (Event.current.button == 0) ? drawSelected : null;
            else if (editMode == MapEditMode.Area)
                value.Area = (Event.current.button == 0) ? drawArea : 0;
            else if (editMode == MapEditMode.Trigger)
                value.Trigger = (Event.current.button == 0) ? drawTrigger : 0;
            else if (editMode == MapEditMode.Height && Event.current.type == EventType.MouseDown)
                value.Height += Event.current.button == 0 ? 1 : -1;
            else if (editMode == MapEditMode.Spawn && Event.current.type == EventType.MouseDown)
                value.SpawnId += Event.current.button == 0 ? 1 : -1;

            GUI.changed = true;
            Event.current.Use();
        }

        if (value?.Prefab != null)
        {
            GUI.DrawTexture(rect, value.Prefab.EditorImage);
            if (editMode == MapEditMode.Area)
                GUI.Label(rect, value.Area.ToString());
            if (editMode == MapEditMode.Height)
                GUI.Label(rect, value.Height.ToString());
            if (editMode == MapEditMode.Trigger && value.Trigger > 0)
                GUI.Label(rect, value.Trigger.ToString());
            if (editMode == MapEditMode.Spawn && value.SpawnId > 0)
                GUI.Label(rect, value.SpawnId.ToString());
        }
        return value;
    }
}
#endif