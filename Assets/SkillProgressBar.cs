using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBar : MonoBehaviour
{

    [SerializeField] SkillProgressBarItem itemPrefab;
    [SerializeField] CardDisplay cardItem;

    [SerializeField] Button confirmButton, skipButton;

    public SkillProgressBarItem Get(int i)
    {
        return transform.GetChild(transform.childCount - i - 2).GetComponent<SkillProgressBarItem>();
    }

    void Update()
    {
        foreach (var item in GetComponentsInChildren<SkillProgressBarItem>(false))
        {
            if (item.State == Skill.Progress.Pending || item.State == Skill.Progress.Now)
            {
                confirmButton.interactable = false;
                return;
            }
        }
        confirmButton.interactable = true;
    }

    public void SetSkills(Card card, GameSettings.PlayerInfo player, Action onSkip, bool allowSkipCard)
    {
        foreach (var skill in card.Skills.Reverse())
        {
            var item = GameObject.Instantiate(itemPrefab);
            item.transform.SetParent(GetComponent<RectTransform>());
            item.SetSkill(skill, onSkip);
        }
        cardItem.SetCard(card);
        cardItem.transform.SetAsLastSibling();
        cardItem.PlayerColorBorder.color = player.Color;
        skipButton.gameObject.SetActive(allowSkipCard);
    }

    internal void Clear()
    {
        foreach (var item in GetComponentsInChildren<SkillProgressBarItem>(true))
            Destroy(item.gameObject);
    }
}
