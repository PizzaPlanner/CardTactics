using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourcePanel : VisualElement
{
    public ResourcePanel()
    {
        this.AddToClassList("resource-panel");
    }

    public ResourceInfo Resources
    {
        get => resources;
        set
        {
            resources = value;
            Clear();

            foreach (var res in resources.Resources)
            {
                var row = new VisualElement();
                row.AddToClassList("resource-panel-group");
                Add(row);
                if (res.Total == 0)
                    row.Add(new CostIcon { Class = res.Type, Rank = -1 });
                else
                    for (int i = 0; i < res.Total; i++)
                        row.Add(new CostIcon { Class = res.Type, Rank = i });
            }
            SetAffordable();
        }
    }

    ResourceInfo resources;

    void SetAffordable()
    {
        this.Query<CostIcon>().ForEach(x => x.SetCostColor(resources));
    }

    public void SetAffordable(ResourceInfo other)
    {
        var ri = new ResourceInfo();
        ri.Resources = new();
        foreach (var res in resources.Resources)
        {
            var res2 = res;
            var available = other.Resources.Sum(x => x.Type == res2.Type ? x.Available : 0);
            res2.Used = Mathf.Max(res2.Available - available, 0);
            ri.Resources.Add(res2);
        }
        this.Query<CostIcon>().ForEach(x => x.SetCostColor(ri));
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