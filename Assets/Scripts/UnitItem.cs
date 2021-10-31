using System.Collections.Generic;
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
            this.Query<Label>("Name").ForEach(x => x.text = unit.Name);

            this.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
            this.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave());

            this.Query<Label>("Life").ForEach(x => x.text = unit.Life.ToString());
            this.Query<Label>("MaxLife").ForEach(x => x.text = unit.MaxLife.ToString());

            unit.OnLifeChange = (life) => this.Query<Label>("Life").ForEach(x => x.text = life.ToString());
        }
    }
    Unit unit;

    void OnMouseEnter()
    {
        if (Unit.Card != null)
            CardTooltip.Show(Unit.Card);
    }

    void OnMouseLeave()
    {
        CardTooltip.Hide();
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