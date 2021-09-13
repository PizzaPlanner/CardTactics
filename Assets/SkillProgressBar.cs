using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBar : MonoBehaviour
{

    [SerializeField] SkillProgressBarItem itemPrefab;

    [SerializeField] Button confirmButton, skipButton;

    public SkillProgressBarItem Get(int i)
    {
        return transform.GetChild(transform.childCount - i - 1).GetComponent<SkillProgressBarItem>();
    }

    void Update()
    {
        foreach (var item in GetComponentsInChildren<SkillProgressBarItem>(false))
        {
            if (item.State == SkillProgressBarItem.Progress.Pending || item.State == SkillProgressBarItem.Progress.Now)
            {
                confirmButton.interactable = false;
                return;
            }
        }
        confirmButton.interactable = true;
    }

    public void SetSkills(Skill[] skills, Action onSkip, bool allowSkipCard)
    {
        foreach (var skill in skills.Reverse())
        {
            var item = GameObject.Instantiate(itemPrefab);
            item.transform.SetParent(GetComponent<RectTransform>());
            item.SetSkill(skill, onSkip);
        }
        skipButton.gameObject.SetActive(allowSkipCard);
    }

    internal void Clear()
    {
        foreach (var item in GetComponentsInChildren<SkillProgressBarItem>(true))
            Destroy(item.gameObject);
    }
}
