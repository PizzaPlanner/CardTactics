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

        if (SkillPanel != null)
        {
            foreach (var leftover in SkillPanel.GetComponentsInChildren<SkillItem>())
                Destroy(leftover.gameObject);
            foreach (var skill in Reference.Skills)
            {
                var item = GameObject.Instantiate(SkillPrefab);
                item.Skill = skill;
                item.FromOtherSource = skill.Source != Reference && skill.Source != null;
                item.transform.SetParent(SkillPanel);
            }
        }
    }
}
