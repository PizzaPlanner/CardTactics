using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class ActionDisplay : MonoBehaviour
{
    [SerializeField] RectTransform TempImage;

    [SerializeField] RectTransform CostPrefab;

    [SerializeField] RectTransform CostPanel;

    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as ActionCard;
        if (Reference == null) return;

        if (TempImage != null)
            TempImage.gameObject.SetActive(Reference.Temporary);

        if (CostPanel != null && CostPrefab != null)
        {
            foreach (Transform t in CostPanel)
                Destroy(t.gameObject);
            foreach (var cost in Reference.Cost)
            {
                for (int i = 0; i < cost.Cost; i++)
                {
                    var citem = Instantiate(CostPrefab);
                    citem.GetComponentInChildren<Image>().sprite = cost.Class.Icon;
                    citem.SetParent(CostPanel);
                }
            }
            CostPanel.gameObject.SetActive(Reference.Cost.Length > 0);
        }

    }

    public void SetCostColor(Dictionary<CardClass, int> free)
    {
        var Reference = GetComponent<CardDisplay>().Reference as ActionCard;
        if (Reference == null) return;

        var panelI = 0;
        foreach (var cost in Reference.Cost)
        {
            for (int i = 0; i < cost.Cost; i++)
            {
                var citem = CostPanel.GetChild(panelI++);
                citem.GetComponentInChildren<Image>().color = free[cost.Class] > i ? Color.white : Color.red;
            }
        }
    }
}
