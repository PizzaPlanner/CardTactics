using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitItem : VisualElement
{
    public Unit Unit
    {
        get => unit;
        set
        {
            unit = value;
            this.Q("Icon").style.backgroundImage = new StyleBackground(unit.Icon);
            this.Query<Label>("Speed").ForEach(x => x.text = unit.TryGetComponent<Actor>(out var actor) ? actor.Speed.ToString() : "0");
            this.Query<Label>("Name").ForEach(x => x.text = unit.Name);

            this.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
            this.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave());

            this.Query<Label>("Life").ForEach(x => x.text = unit.DisplayLife.ToString());
            this.Query<VisualElement>("Lifebar-Bar").ForEach(x => x.style.width = new StyleLength(new Length(unit.DisplayLife / unit.MaxLife * 100, LengthUnit.Percent)));
        }
    }
    Unit unit;

    void OnMouseEnter()
    {
        var t = Tile.FindObjectsOfType<Tile>().FirstOrDefault(x => x.Unit == unit);
        if (t != null)
            t.Hover();
    }

    void OnMouseLeave()
    {
        if (Tile.Hovered == null) return;
        Tile.Hovered.OnMouseLeave();
    }


    public new class UxmlFactory : UxmlFactory<UnitItem, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }

}