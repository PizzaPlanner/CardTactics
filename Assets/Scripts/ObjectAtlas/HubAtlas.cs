using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class HubAtlas : ObjectAtlas<Hubworld>
{

#if UNITY_EDITOR
    protected override void Populate()
    {
        var objs = new List<Hubworld>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:prefab", new[] { "Assets/Maps" });
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            foreach (var item in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
                if (item is Hubworld map)
                    objs.Add(map);
        }
        Objects = objs.ToArray();
    }
#endif
}