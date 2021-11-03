using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class CostIcon : VisualElement
{
    public CostIcon()
    {
        AddToClassList("cost-icon");
        AddToClassList("cost-icon--affordable");
        var child = new VisualElement();
        child.AddToClassList("cost-icon-icon");
        Add(child);
    }

    public CardClass Class
    {
        get => cls;
        set
        {
            cls = value;
            this.Q(null, "cost-icon-icon").style.backgroundImage = new StyleBackground(cls.Icon);
        }
    }

    CardClass cls;

    public void Set(CardClass cardClass)
    {
        Class = cardClass;
    }

    public void SetCostColor(ResourceInfo resources)
    {
        if (Class == null) return;
        bool affordable = resources.Classes.Contains(Class);
        RemoveFromClassList(affordable ? "cost-icon--overbudget" : "cost-icon--affordable");
        AddToClassList(affordable ? "cost-icon--affordable" : "cost-icon--overbudget");
    }

    public new class UxmlFactory : UxmlFactory<CostIcon, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }
}
