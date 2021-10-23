using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Tilemaps;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

class TMET : TilemapEditorTool
{
    protected override string tooltipStringFormat => "--->";

    protected override string shortcutId => "o";
}

[Overlay(typeof(SceneView), "Map Brushes")]
public class BrushOverlay : Overlay
{
    GridBrushBase Current;
    List<GridBrushBase> Brushes;

    VisualElement Content, Toolbelt;
    Map map;
    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement() { name = "Map Brushes" };
        Selection.selectionChanged += () =>
        {
            OnSelectionChange(root);
        };
        Content = new VisualElement();
        Content.style.maxWidth = 200;
        root.Add(Content);
        Toolbelt = new VisualElement();
        Toolbelt.style.flexDirection = FlexDirection.Row;
        var pbtn = new Button(() =>
        {
            TMET.CreateInstance<TMET>();
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
        })
        { text = "+" };
        Toolbelt.Add(pbtn);
        var dbtn = new Button(() =>
        {
            TMET.CreateInstance<TMET>();
            TilemapEditorTool.SetActiveEditorTool(typeof(EraseTool));
        })
        { text = "-" };
        Toolbelt.Add(dbtn);
        Brushes = new List<GridBrushBase>();
        foreach (var brush in GetType().Assembly.GetTypes().Where(x => typeof(GridBrushBase).IsAssignableFrom(x)))
        {
            GridBrushBase brushItem = (GridBrushBase)ScriptableObject.FindObjectOfType(brush);
            if (brushItem == null)
                brushItem = (GridBrushBase)ScriptableObject.CreateInstance(brush.FullName);
            Brushes.Add(brushItem);
            root.Add(new Button(() =>
            {
                Current = brushItem;
                Redraw();
            })
            { text = brush.Name });
        }
        OnSelectionChange(root);
        return root;
    }

    void OnSelectionChange(VisualElement root)
    {
        var go = Selection.activeGameObject;
        if (go == null || !go.TryGetComponent<Map>(out var map))
            root.style.display = DisplayStyle.None;
        else
        {
            root.style.display = DisplayStyle.Flex;
            this.map = map;
        }
    }

    void Redraw()
    {
        Content.Clear();
        Content.Add(Toolbelt);
        var insp = new InspectorElement(Current);
        map.GetComponent<Grid>();
        Content.Add(insp);
    }
}