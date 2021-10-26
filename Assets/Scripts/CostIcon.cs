using System.Collections.Generic;
using UnityEngine.UIElements;

public class CostIcon : VisualElement
{
    public CostIcon()
    {
        AddToClassList("cost-icon");
        AddToClassList("costicon-affordable");
    }

    public CardClass Class
    {
        get => cls;
        set
        {
            cls = value;
            style.backgroundImage = new StyleBackground(cls.Icon);
        }
    }

    CardClass cls;
    public int Rank;

    public void Set(CardClass cardClass, int rank)
    {
        Class = cardClass;
        Rank = rank;
    }

    public void SetCostColor(Dictionary<CardClass, int> free)
    {
        if (Class == null) return;
        bool affordable = free.ContainsKey(Class) && free[Class] > Rank;
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
