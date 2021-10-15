using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SkillProgressBarItem : MonoBehaviour
{

    const float SKIP_TIME = 0.5f;
    public Skill.Progress State => skill.PlayProgress;

    public RectTransform skipPanel;
    public RectTransform skipBar;
    float skipDuration;
    Action onSkip;

    public Sprite Pending;
    public Sprite Queued;
    public Sprite Skipped;
    public Sprite Failed;
    public Sprite Now;

    Skill skill;

    public Func<bool> ShouldUpdate;

    public Image Icon;

    [SerializeField] AudioClip SkipSound, FailSound;

    public void SetSkill(Skill skill, Action OnSkip)
    {
        this.skill = skill;
        Icon.sprite = skill.Icon;
        onSkip = OnSkip;
        GetComponent<Image>().sprite = Pending;
        skipBar.gameObject.SetActive(false);
        skipPanel.gameObject.SetActive(false);
    }

    Skill.Progress prevProgress = Skill.Progress.Pending;

    void Update()
    {
        var skippable = skill.IsOptional && skill.PlayProgress == Skill.Progress.Now && onSkip != null;
        if ((skill.PlayProgress == prevProgress && !skippable) || !ShouldUpdate())
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

        if (skill.PlayProgress == Skill.Progress.Skipped)
            GetComponent<AudioSource>().PlayOneShot(SkipSound);
        else if (skill.PlayProgress == Skill.Progress.Failed)
            GetComponent<AudioSource>().PlayOneShot(FailSound);

        skipPanel.gameObject.SetActive(skippable);
        if (!skippable || !Input.GetButton("Skip"))
        {
            skipBar.gameObject.SetActive(false);
            skipDuration = 0f;
        }
        else if (Input.GetButton("Skip"))
        {
            skipDuration += Time.deltaTime / SKIP_TIME;
            skipBar.gameObject.SetActive(true);
            if (skipDuration >= 1)
            {
                skipDuration = 1;
                onSkip();
                skipBar.gameObject.SetActive(false);
            }
            skipBar.anchorMax = new Vector2(skipDuration, 1);
        }
    }
}
