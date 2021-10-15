using System;
using System.Linq;
using UnityEngine;

public class SkillProgressBar : MonoBehaviour
{
    [SerializeField] SkillProgressBarItem itemPrefab;
    [SerializeField] RectTransform skillPanel;

    Card card;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (card.State == Card.CardState.Current)
        {
            animator.SetTrigger("Start");
            animator.ResetTrigger("Next");
            // transform.localScale = Vector3.one * 1.15f;
        }
        else
        {
            animator.SetTrigger("Next");
            animator.ResetTrigger("Start");
            // transform.localScale = Vector3.one;
        }
    }

    public void SetSkills(Card card, PlayerInfo player)
    {
        this.card = card;
        foreach (var skill in card.Skills.Reverse())
        {
            var item = GameObject.Instantiate(itemPrefab);
            item.transform.SetParent(skillPanel);
            item.SetSkill(skill, () => card.WantSkillSkip = true);
            item.ShouldUpdate = () => card.State == Card.CardState.Current;
        }
        var cardItem = GetComponent<CardDisplay>();
        cardItem.SetCard(card);
        // cardItem.PlayerColorBorder.color = player.Color;
        // skipButton.gameObject.SetActive(false);
    }
}
