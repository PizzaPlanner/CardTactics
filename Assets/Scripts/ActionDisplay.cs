using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class ActionDisplay : MonoBehaviour
{
    [SerializeField] RectTransform TempImage;

    [SerializeField] CostIcon CostPrefab;

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
                    citem.transform.SetParent(CostPanel);
                    citem.Class = cost.Class;
                    citem.Rank = i;
                }
            }
            CostPanel.gameObject.SetActive(Reference.Cost.Length > 0);
        }
    }
}
