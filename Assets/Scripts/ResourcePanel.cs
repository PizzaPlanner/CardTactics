using System.Collections.Generic;
using System.Linq;
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

            foreach (var res in resources.Classes)
            {
                Add(new CostIcon { Class = res });
            }
        }
    }

    ResourceInfo resources;

    public void SetAffordable(ResourceInfo info)
    {
        this.Query<CostIcon>().ForEach(x => x.SetCostColor(info));
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