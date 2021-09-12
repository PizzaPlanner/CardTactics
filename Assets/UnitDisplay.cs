using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitDisplay : MonoBehaviour
{
    public UnitCard Reference;

    [SerializeField] Text Life, MaxLife, Passive;

    void OnEnable()
    {
        if (Reference == null) return;

        if (Life != null)
            Life.text = Reference.UnitReference.LifePreview.ToString();
        if (MaxLife != null)
            MaxLife.text = Reference.UnitReference.MaxLife.ToString();
        if (Passive != null)
            Passive.text = Reference.Passive != null ? Reference.Passive.Description : "";
    }
}
