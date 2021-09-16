using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
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

    [SerializeField] AudioClip OkSound, SkipSound, FailSound;

    public void SetSkill(Skill skill, Action OnSkip)
    {
        this.skill = skill;
        Icon.sprite = skill.Icon;
        skipButton.onClick.RemoveAllListeners();
        if (skill.IsOptional)
            skipButton.onClick.AddListener(() => OnSkip());
    }

    Skill.Progress prevProgress = Skill.Progress.Pending;

    void Update()
    {
        if (skill.PlayProgress == prevProgress)
            return;

        prevProgress = skill.PlayProgress;
        GetComponent<Image>().sprite = skill.PlayProgress switch
        {
            Skill.Progress.Pending => Pending,
            Skill.Progress.Queued => Queued,
            Skill.Progress.Skipped => Skipped,
            Skill.Progress.Failed => Failed,
            Skill.Progress.Now => Now,
        };
        skipButton.gameObject.SetActive(skill.IsOptional && skill.PlayProgress == Skill.Progress.Now);
        if (skill.PlayProgress == Skill.Progress.Queued)
            GetComponent<AudioSource>().PlayOneShot(OkSound);
        else if (skill.PlayProgress == Skill.Progress.Skipped)
            GetComponent<AudioSource>().PlayOneShot(SkipSound);
        else if (skill.PlayProgress == Skill.Progress.Failed)
            GetComponent<AudioSource>().PlayOneShot(FailSound);
    }
}
