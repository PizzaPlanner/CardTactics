using UnityEngine;

[RequireComponent(typeof(CardDisplay))]
public class CardTooltip : MonoBehaviour
{
    static CardTooltip Tooltip;
    static CardDisplay Display;
    public RectTransform RelatedCardsDisplay;
    public CardDisplay RelatedCardPrefab;

    void Start()
    {
        Tooltip = this;
        Display = GetComponent<CardDisplay>();
        gameObject.SetActive(false);
    }

    public static void Show(Card card)
    {
        Display.SetCard(card);
        Tooltip.ShowRelated(card);
        Tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Tooltip.gameObject.SetActive(false);
    }

    public void ShowRelated(Card card)
    {
        foreach (Transform t in RelatedCardsDisplay)
            Destroy(t.gameObject);

        foreach (var related in card.GetRelatedCards())
        {
            var c = Instantiate(RelatedCardPrefab);
            c.transform.SetParent(RelatedCardsDisplay);
            c.SetCard(related);
        }

        RelatedCardsDisplay.gameObject.SetActive(RelatedCardsDisplay.childCount > 0);
    }
}
