using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CostIcon : MonoBehaviour
{
    public CardClass Class;
    public int Rank;

    void Start()
    {
        if (Class != null)
            GetComponent<Image>().sprite = Class.Icon;
    }

    public void SetCostColor(Dictionary<CardClass, int> free)
    {
        if (Class == null) return;
        bool affordable = free.ContainsKey(Class) && free[Class] > Rank;
        GetComponent<Image>().color = affordable ? Color.white : Color.red;
    }
}
