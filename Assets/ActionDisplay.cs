using System;
using UnityEngine;

[RequireComponent(typeof(CardDisplay))]
public class ActionDisplay : MonoBehaviour
{
    [SerializeField] RectTransform TempImage, SkillPanel;

    [SerializeField] SkillItem SkillPrefab;

    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as ActionCard;
        if (Reference == null) return;

        if (TempImage != null)
            TempImage.gameObject.SetActive(Reference.Temporary);
    }
}
