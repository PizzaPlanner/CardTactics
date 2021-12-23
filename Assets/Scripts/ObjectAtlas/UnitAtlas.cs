using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitAtlas : ObjectAtlas<Unit>
{

#if UNITY_EDITOR
    protected override void Populate()
    {
        var objs = new List<Unit>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:prefab", new[] { "Assets/Units" });
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            foreach (var item in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
                if (item is Unit unit)
                    objs.Add(unit);
        }
        Objects = objs.ToArray();
    }
#endif
}