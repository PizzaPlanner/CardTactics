using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MapAtlas : ObjectAtlas<Map>
{

#if UNITY_EDITOR
    protected override void Populate()
    {
        var objs = new List<Map>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:prefab", new[] { "Assets/Maps" });
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            foreach (var item in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
                if (item is Map map)
                    objs.Add(map);
        }
        Objects = objs.ToArray();
    }
#endif
}