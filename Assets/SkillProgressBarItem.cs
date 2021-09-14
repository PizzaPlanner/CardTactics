using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBarItem : MonoBehaviour
{

    public Skill.Progress State => skill.PlayProgress;

    public Button skipButton;

    public Sprite Pending;
    public Sprite Queued;
    public Sprite Skipped;
    public Sprite Failed;
    public Sprite Now;

    Skill skill;

    public Image Icon;

    public void SetSkill(Skill skill, Action OnSkip)
    {
        this.skill = skill;
        Icon.sprite = skill.Icon;
        skipButton.onClick.RemoveAllListeners();
        if (skill.IsOptional)
            skipButton.onClick.AddListener(() => OnSkip());
    }

    void Update()
    {
        GetComponent<Image>().sprite = skill.PlayProgress switch
        {
            Skill.Progress.Pending => Pending,
            Skill.Progress.Queued => Queued,
            Skill.Progress.Skipped => Skipped,
            Skill.Progress.Failed => Failed,
            Skill.Progress.Now => Now,
        };
        skipButton.gameObject.SetActive(skill.IsOptional && skill.PlayProgress == Skill.Progress.Now);
    }
}
