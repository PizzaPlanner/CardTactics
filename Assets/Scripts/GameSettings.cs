using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private List<TKey> keyData = new List<TKey>();

    [SerializeField, HideInInspector]
    private List<TValue> valueData = new List<TValue>();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
        {
            this[this.keyData[i]] = this.valueData[i];
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        this.keyData.Clear();
        this.valueData.Clear();

        foreach (var item in this)
        {
            this.keyData.Add(item.Key);
            this.valueData.Add(item.Value);
        }
    }
}

[System.Serializable] public class ColorDictionary : UnitySerializedDictionary<TargetIndicator, Color> { }

[CreateAssetMenu(menuName = "Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings Instance { get; private set; }

    public void Load()
    {
        Instance = this;
        Debug.Log("[Board] Using Settings: " + name);
    }

    [Header("Colors")]
    public ColorDictionary TileIndicatorColors;
    public ColorDictionary TileIndicatorHighlightColors;
    public ColorDictionary BorderIndicatorColors;
    public ColorDictionary BorderIndicatorHighlightColors;

    [BoxGroup("Timings")]
    public bool UseAnimations;
    [BoxGroup("Timings")]
    public bool SkipTimings;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float BeforePlayDelay;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float AfterPlayDelay;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float TurnAnnouncerTime;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float CardGainAnnouncerTime;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float MarkerDelayTime;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float RevealAreaPreDelay;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float RevealAreaPostDelay;
    [HideIf("SkipTimings")]
    [BoxGroup("Timings/Times")]
    public float EndGameDelay;
}

public static class GameSettingsHelper
{
    public static YieldInstruction Yield(this float f) => GameSettings.Instance.SkipTimings ? null : new WaitForSeconds(f);
}
