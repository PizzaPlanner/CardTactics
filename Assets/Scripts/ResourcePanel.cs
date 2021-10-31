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

    public new class UxmlFactory : UxmlFactory<ResourcePanel, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }

}