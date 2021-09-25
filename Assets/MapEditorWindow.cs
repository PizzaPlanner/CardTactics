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
        Marker,
        Unit
    }
    EnumField mapEditMode;
    ObjectField drawSelected;
    IntegerField drawArea;

    Map.MapInfo data;
    VisualElement map, root;
    ObjectField drawUnit;

    bool clickmap;

    Map reference;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        reference = property.serializedObject.targetObject as Map;
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
        drawUnit = new ObjectField() { objectType = typeof(Unit), allowSceneObjects = false };
        mapedit.Add(mapEditMode);
        mapedit.Add(drawSelected);
        mapedit.Add(drawArea);
        mapedit.Add(drawUnit);
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
            AssetDatabase.SaveAssets();
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
                var elm = new TileInfoItem(x, y, this);
                row.Add(elm);
            }
        }
    }

    class TileInfoItem : VisualElement
    {
        Map.TilePrefabWithInfo info;
        Vector2Int position;
        Label Area;
        VisualElement UnitIcon;
        public TileInfoItem(int x, int y, TileGridDrawer drawer)
        {
            this.info = drawer.data[x, y];
            this.position = new Vector2Int(x, y);
            this.style.width = 32;
            this.style.height = 32;
            this.style.borderLeftColor = Color.black * new Color(1, 1, 1, 0.4f);
            this.style.borderRightColor = Color.black * new Color(1, 1, 1, 0.4f);
            this.style.borderTopColor = Color.black * new Color(1, 1, 1, 0.4f);
            this.style.borderBottomColor = Color.black * new Color(1, 1, 1, 0.4f);
            this.style.borderLeftWidth = 1;
            this.style.borderRightWidth = 1;
            this.style.borderTopWidth = 1;
            this.style.borderBottomWidth = 1;
            this.style.justifyContent = Justify.Center;
            this.style.alignItems = Align.Center;
            UnitIcon = new VisualElement();
            UnitIcon.style.position = Position.Absolute;
            UnitIcon.style.width = 24;
            UnitIcon.style.height = 24;
            UnitIcon.style.borderLeftWidth = 1;
            UnitIcon.style.borderRightWidth = 1;
            UnitIcon.style.borderTopWidth = 1;
            UnitIcon.style.borderBottomWidth = 1;
            Add(UnitIcon);
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
                        info.Owner = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Trigger:
                        info.Trigger = evt.button == 0 ? drawer.drawArea.value : 0;
                        break;
                    case MapEditMode.Unit:
                        info.UnitPrefab = evt.button == 0 ? (Unit)drawer.drawUnit.value : null;
                        break;
                }
                Update(drawer);
            });
            Update(drawer);
        }

        void CheckPassable(TileGridDrawer drawer)
        {
            bool hdiff(Map.TilePrefabWithInfo other) => Math.Abs((other.Prefab == null ? -9999 : other.Height) - (info.Prefab == null ? -9999 : info.Height)) > 1;
            if (position.x > 0)
                this.style.borderLeftColor = Color.black * new Color(1, 1, 1, hdiff(drawer.data[position.x - 1, position.y]) ? 1f : 0.1f);
            if (position.x < drawer.data.Size.x - 1)
                this.style.borderRightColor = Color.black * new Color(1, 1, 1, hdiff(drawer.data[position.x + 1, position.y]) ? 1f : 0.1f);
            if (position.y < drawer.data.Size.y - 1)
                this.style.borderTopColor = Color.black * new Color(1, 1, 1, hdiff(drawer.data[position.x, position.y + 1]) ? 1f : 0.1f);
            if (position.y > 0)
                this.style.borderBottomColor = Color.black * new Color(1, 1, 1, hdiff(drawer.data[position.x, position.y - 1]) ? 1f : 0.1f);
        }

        public void Update(TileGridDrawer drawer)
        {
            CheckPassable(drawer);
            style.backgroundImage = info.Prefab?.EditorImage;
            if (info.Prefab == null)
            {
                Area.style.visibility = Visibility.Hidden;
                UnitIcon.style.visibility = Visibility.Hidden;
                return;
            }
            Area.style.visibility = Visibility.Visible;
            UnitIcon.style.visibility = Visibility.Visible;
            if (info.Owner > 0 && info.Owner <= drawer.reference.Players.Length)
            {
                UnitIcon.style.borderBottomColor = drawer.reference.Players[info.Owner - 1].Color;
                UnitIcon.style.borderTopColor = drawer.reference.Players[info.Owner - 1].Color;
                UnitIcon.style.borderRightColor = drawer.reference.Players[info.Owner - 1].Color;
                UnitIcon.style.borderLeftColor = drawer.reference.Players[info.Owner - 1].Color;
            }
            else
            {
                UnitIcon.style.borderBottomColor = Color.clear;
                UnitIcon.style.borderTopColor = Color.clear;
                UnitIcon.style.borderRightColor = Color.clear;
                UnitIcon.style.borderLeftColor = Color.clear;
            }

            if (info.UnitPrefab != null)
                UnitIcon.style.backgroundImage = info.UnitPrefab.Icon.texture;
            else
                UnitIcon.style.backgroundImage = null;

            switch (drawer.mapEditMode.value)
            {
                case MapEditMode.Unit:
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
                    Area.text = info.Owner == 0 ? "" : info.Owner.ToString();
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
        elm.Add(new PropertyField(serializedObject.FindProperty("TilePrefabs")));
        elm.Add(new PropertyField(serializedObject.FindProperty("Triggers")));
        elm.Add(new PropertyField(serializedObject.FindProperty("Players")));
        return elm;
    }
}
#endif