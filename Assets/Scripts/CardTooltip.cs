using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardTooltip : MonoBehaviour
{
    static CardTooltip Instance => instance ??= FindObjectOfType<CardTooltip>(true);
    static CardTooltip instance;

    CardItem mainCard;

    void Start()
    {
        instance = this;
        mainCard = FindObjectOfType<UIDocument>().rootVisualElement.Q("TooltipCard").Q<CardItem>();
        Debug.Assert(mainCard != null, this);
        Hide();
    }

    public static void Show(Card card)
    {
        Instance.mainCard.Card = card;
        Instance.mainCard.style.display = DisplayStyle.Flex;
        Instance.ShowRelated(card);
    }

    public static void Hide()
    {
        Instance.mainCard.style.display = DisplayStyle.None;
    }

    public void ShowRelated(Card card)
    {
        // foreach (Transform t in RelatedCardsDisplay)
        //     Destroy(t.gameObject);

        // foreach (var related in card.GetRelatedCards().Distinct(new CardComparer()))
        // {
        //     if (related.Name.Equals(card.Name)) continue;

        // }

        // RelatedCardsDisplay.gameObject.SetActive(RelatedCardsDisplay.childCount > 0);
    }
}

public class CardComparer : IEqualityComparer<Card>
{
    public bool Equals(Card x, Card y) => x.Name.Equals(y.Name);

    public int GetHashCode(Card obj) => obj.GetHashCode();
}
