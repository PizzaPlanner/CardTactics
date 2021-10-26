using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class ResourcePanel : VisualElement
{
    public ResourcePanel()
    {
        this.AddToClassList("resource-panel");
    }

    public Deck Deck
    {
        get => deck;
        set
        {
            deck = value;
            Clear();

            var total = deck.GetTotalCapacities();
            foreach (var res in total)
            {
                var row = new VisualElement();
                row.AddToClassList("resource-panel-group");
                Add(row);
                for (int i = 0; i < res.Value; i++)
                    row.Add(new CostIcon { Class = res.Key, Rank = i });
            }
        }
    }

    Deck deck;

    public void Update()
    {
        var used = deck.GetUsedCapacities();
        var total = deck.GetTotalCapacities();

        foreach (var item in total.Keys)
            total[item] -= used[item];

        this.Query<CostIcon>().ForEach(x => x.SetCostColor(used));
    }

    public new class UxmlFactory : UxmlFactory<ResourcePanel, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }

}