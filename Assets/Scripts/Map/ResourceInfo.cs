using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ResourceInfo
{
    [TableList(AlwaysExpanded = true)]
    public List<Resource> Resources;

    public bool CanAfford(ResourceInfo other)
    {
        foreach (var res in other.Resources)
            if (!Resources.Any(x => x.Type == res.Type && x.Available >= res.Available)) return false;
        return true;
    }

    public void Spend(ResourceInfo other)
    {
        foreach (var otherRes in other.Resources)
            for (int i = 0; i < Resources.Count; i++)
                if (Resources[i].Type == otherRes.Type)
                {
                    Resources[i] = new Resource
                    {
                        Type = Resources[i].Type,
                        Total = otherRes.Type.OneTimeUse ? Resources[i].Total - otherRes.Available : Resources[i].Total,
                        Used = otherRes.Type.OneTimeUse ? 0 : Resources[i].Used + otherRes.Available,
                    };

                    if (Resources[i].Total <= 0 && Resources[i].Type.OneTimeUse)
                        Resources.RemoveAt(i--);
                }
    }

    public void Refresh()
    {
        for (int i = 0; i < Resources.Count; i++)
            Resources[i] = new Resource
            {
                Type = Resources[i].Type,
                Total = Resources[i].Total,
                Used = 0,
            };
    }

    public void Add(CardClass type, int amount)
    {
        Debug.Log($"Add {amount} {type.Name}");
        bool found = false;
        for (int i = 0; i < Resources.Count; i++)
            if (Resources[i].Type == type)
            {
                var r2 = Resources[i];
                r2.Total += amount;
                Resources[i] = r2;
                found = true;
                break;
            }
        if (!found)
            Resources.Add(new Resource { Type = type, Total = amount });
    }

    public void Remove(CardClass type, int amount)
    {
        for (int i = 0; i < Resources.Count; i++)
            if (Resources[i].Type == type && amount > 0)
            {
                var r2 = Resources[i];
                r2.Total -= amount;
                r2.Used = Mathf.Max(0, r2.Used - amount);
                amount = -r2.Total;
                Resources[i] = r2;
                if (r2.Total <= 0)
                {
                    Resources.Remove(r2);
                    i--;
                }
            }
    }
}
