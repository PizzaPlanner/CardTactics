#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Trigger))]
public class TriggerDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (property.managedReferenceValue != null)
        {
            return new PropertyField(property) { label = property.managedReferenceValue.GetType().Name };
        }
        else
        {
            return new UnityEngine.UIElements.Button(() =>
            {
                GenericMenu gm = new GenericMenu();
                foreach (var type in typeof(Trigger).Assembly.GetTypes().Where(x => typeof(Trigger).IsAssignableFrom(x) && !x.IsAbstract))
                {
                    gm.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        property.serializedObject.Update();
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                gm.ShowAsContext();
            })
            { text = "Assign Type" };
        }
    }
}

[CustomPropertyDrawer(typeof(MultiTrigger))]
public class MultiTriggerDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = Activator.CreateInstance(typeof(MultiTrigger));
            property.serializedObject.ApplyModifiedProperties();
        }
        return new PropertyField(property.FindPropertyRelative("SubTriggers")) { label = property.managedReferenceValue.GetType().Name };
    }
}

[CustomPropertyDrawer(typeof(Map.MapInfo))]
public class TileGridDrawer : PropertyDrawer
{

    enum MapEditMode
    {
        Tile,
        Area,
        Trigger,
        Height,
        Spawn,
        Marker
    }
    EnumField mapEditMode;
    ObjectField drawSelected;
    IntegerField drawArea;

    Map.MapInfo data;
    VisualElement map, root;

    bool clickmap;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = new Map.MapInfo();
            property.serializedObject.ApplyModifiedProperties();
        }
        data = (Map.MapInfo)property.managedReferenceValue;
        if (data.Size == Vector2Int.zero)
        {
            data.Size = Vector2Int.one;
            data[1, 1] = new Map.TilePrefabWithInfo();
        }
        root = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        var mapedit = new VisualElement();
        mapedit.style.flexDirection = FlexDirection.Row;
        mapEditMode = new EnumField(MapEditMode.Tile);
        mapEditMode.RegisterValueChangedCallback(_ => map.Query<TileInfoItem>().ForEach(y => y.Update(this)));
        drawArea = new IntegerField();
        drawSelected = new ObjectField() { objectType = typeof(Tile), allowSceneObjects = false };
        mapedit.Add(mapEditMode);
        mapedit.Add(drawSelected);
        mapedit.Add(drawArea);
        root.Add(mapedit);
        map = new VisualElement();
        map.style.flexDirection = FlexDirection.ColumnReverse;
        map.RegisterCallback<MouseDownEvent>(_ => clickmap = true);
        map.RegisterCallback<MouseUpEvent>(_ => clickmap = false);
        root.Add(map);
        var bottom = new VisualElement();
        bottom.style.flexDirection = FlexDirection.Row;
        bottom.style.justifyContent = Justify.SpaceBetween;
        root.Add(bottom);
        bottom.Add(new Button(() =>
        {
            data.Size += new Vector2Int(0, 1);
            property.serializedObject.Update();
            property.managedReferenceValue = data;
            property.serializedObject.ApplyModifiedProperties();
            DrawMap();
        })
        { text = "+ y" });
        bottom.Add(new Button(() =>
        {
            property.serializedObject.Update();
            property.managedReferenceValue = data;
            property.serializedObject.ApplyModifiedProperties();
        })
        { text = "Save" });
        bottom.Add(new Button(() =>
        {
            data.Size += new Vector2Int(1, 0);
            property.serializedObject.Update();
            property.managedReferenceValue = data;
            property.serializedObject.ApplyModifiedProperties();
            DrawMap();
        })
        { text = "+ x" });
        DrawMap();
        return root;
    }

    void DrawMap()
    {
        map.Clear();
        for (int y = 0; y < data.Size.y; y++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            map.Add(row);
            for (int x = 0; x < data.Size.x; x++)
            {
                var elm = new TileInfoItem(data[x, y], this);
                row.Add(elm);
            }
        }
    }

    class TileInfoItem : VisualElement
    {
        Map.TilePrefabWithInfo info;
        Label Area;
        public TileInfoItem(Map.TilePrefabWithInfo info, TileGridDrawer drawer)
        {
            this.info = info;
            this.style.width = 32;
            this.style.height = 32;
            this.style.borderLeftColor = Color.black;
            this.style.borderRightColor = Color.black;
            this.style.borderTopColor = Color.black;
            this.style.borderBottomColor = Color.black;
            this.style.borderLeftWidth = 1;
            this.style.borderRightWidth = 1;
            this.style.borderTopWidth = 1;
            this.style.borderBottomWidth = 1;
            this.style.justifyContent = Justify.Center;
            this.style.alignItems = Align.Center;
            Area = new Label();
            Add(Area);

            RegisterCallback<MouseMoveEvent>((evt) =>
            {
                if (!drawer.clickmap) return;
                switch (drawer.mapEditMode.value)
                {
                    case MapEditMode.Tile:
                        info.Prefab = evt.button == 0 ? (Tile)drawer.drawSelected.value : null;
                        break;
                    case MapEditMode.Area:
                        info.Area = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Height:
                        info.Height = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Marker:
                        info.MarkerId = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Spawn:
                        info.SpawnId = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Trigger:
                        info.Trigger = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                }
                Update(drawer);
            });
            Update(drawer);
        }

        public void Update(TileGridDrawer drawer)
        {
            style.backgroundImage = info.Prefab?.EditorImage;
            if (info.Prefab == null)
            {
                Area.style.visibility = Visibility.Hidden;
                return;
            }
            Area.style.visibility = Visibility.Visible;

            switch (drawer.mapEditMode.value)
            {
                case MapEditMode.Tile:
                    Area.text = "";
                    break;
                case MapEditMode.Area:
                    Area.text = info.Area.ToString();
                    break;
                case MapEditMode.Height:
                    Area.text = info.Height.ToString();
                    break;
                case MapEditMode.Marker:
                    Area.text = info.MarkerId == 0 ? "" : info.MarkerId.ToString();
                    break;
                case MapEditMode.Spawn:
                    Area.text = info.SpawnId == 0 ? "" : info.SpawnId.ToString();
                    break;
                case MapEditMode.Trigger:
                    Area.text = info.Trigger == 0 ? "" : info.Trigger.ToString();
                    break;
            }
        }
    }
}

[CustomEditor(typeof(Map))]
public class MapEditorWindow : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var elm = new VisualElement();
        elm.Add(new PropertyField(serializedObject.FindProperty("Triggers")));
        elm.Add(new PropertyField(serializedObject.FindProperty("TilePrefabs")));
        return elm;
    }
}
#endif