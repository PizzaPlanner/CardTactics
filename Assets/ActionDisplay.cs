using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class ActionDisplay : MonoBehaviour
{
    [SerializeField] RectTransform TempImage, SkillPanel;

    [SerializeField] SkillItem SkillPrefab;

    [SerializeField] Image RequiredClass;
    [SerializeField] Text CostText;

    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as ActionCard;
        if (Reference == null) return;

        if (TempImage != null)
            TempImage.gameObject.SetActive(Reference.Temporary);
        if (CostText != null)
            CostText.text = Reference.Cost.ToString();
        if (RequiredClass != null)
            RequiredClass.sprite = Reference.RequiredClass.Icon;
    }

    void Update()
    {
        if (!TryGetComponent<Button>(out var btn)) return;
        CostText.color = btn.interactable ? Color.white : Color.red;
    }
}
