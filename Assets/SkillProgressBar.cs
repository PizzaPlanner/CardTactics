using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBar : MonoBehaviour
{
    [SerializeField] SkillProgressBarItem itemPrefab;
    [SerializeField] RectTransform skillPanel;

    Card card;

    void Update()
    {
        if (card.State == Card.CardState.Current)
        {
            transform.localScale = Vector3.one * 1.15f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void SetSkills(Card card, GameSettings.PlayerInfo player)
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
        cardItem.PlayerColorBorder.color = player.Color;
        // skipButton.gameObject.SetActive(false);
    }
}
