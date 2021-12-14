using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ObjectAtlas<T> : MonoBehaviour where T : UnityEngine.Object
{
    public static ObjectAtlas<T> Instance => instance ??= FindObjectOfType<ObjectAtlas<T>>();
    static ObjectAtlas<T> instance;

    [SerializeField] T[] Objects;
    public T Item(int i)
    {
        if (i < 0 || i >= Objects.Length)
            return null;
        return Objects[i];
    }
    public bool TryGet(T item, out int index)
    {
        index = -1;
        for (int i = 0; i < Objects.Length; i++)
            if (Objects[i] == item)
            {
                index = i;
                return true;
            }
        return false;
    }


#if UNITY_EDITOR
    void OnValidate()
    {
        Populate();
    }

    [Button]
    void Populate()
    {
        var objs = new List<T>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (var guid in References(guids))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var item = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            objs.Add(item);
        }
        Objects = objs.ToArray();
    }

    protected virtual string[] References(string[] guids)
    {
        var list = new List<string>();
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("*", new[] { "Assets/Cards", "Assets/Maps", "Assets/Decks" }))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".asset") && !path.EndsWith(".prefab"))
                continue;

            var content = System.IO.File.ReadAllText(path);
            foreach (var guid2 in guids)
                if (!list.Contains(guid2) && content.Contains(guid2) && guid != guid2)
                    list.Add(guid2);
        }
        return list.ToArray();
    }
#endif
}

public static class AtlasSerializationHelper
{
    public static void WriteObject<T>(this NetworkWriter writer, T item) where T : Object
    {
        ObjectAtlas<T>.Instance.TryGet(item, out var index);
        writer.Write(index);
    }

    public static T ReadObject<T>(this NetworkReader reader) where T : Object
    {
        return ObjectAtlas<T>.Instance.Item(reader.ReadInt());
    }
}