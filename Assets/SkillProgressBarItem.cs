using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBarItem : MonoBehaviour
{
    public enum Progress
    {
        Pending,
        Queued,
        Skipped,
        Failed,
        Now
    }

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
        SetProgress(Progress.Pending);
        Icon.sprite = skill.Icon;
        skipButton.onClick.RemoveAllListeners();
        if (skill.IsOptional)
            skipButton.onClick.AddListener(() => { SetProgress(Progress.Skipped); OnSkip(); });
    }

    public Progress State { get; private set; }

    internal void SetProgress(Progress progress)
    {
        GetComponent<Image>().sprite = progress switch
        {
            Progress.Pending => Pending,
            Progress.Queued => Queued,
            Progress.Skipped => Skipped,
            Progress.Failed => Failed,
            Progress.Now => Now,
        };
        skipButton.gameObject.SetActive(skill.IsOptional && progress == Progress.Now);
        State = progress;
    }
}
