
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu]
public class CardClass : ScriptableObject
{
    public string Name;

    [Sirenix.OdinInspector.PreviewField(32)]
    [Sirenix.OdinInspector.AssetSelector(Paths = "Assets/3rdParty/64 flat icons/png/64px", FlattenTreeView = true)]
    public Sprite Icon;

    public bool OneTimeUse;

    public override string ToString() => $"<sprite=\"classicons\" name=\"{Name}\">";
}