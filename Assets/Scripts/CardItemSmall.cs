using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardItemSmall : VisualElement
{
    public Card Card
    {
        get => card;
        set
        {
            card = value;
            this.Q("Icon").style.backgroundImage = new StyleBackground(card.Icon);
            this.Q<Label>("Speed").text = card.Speed.ToString();

            this.Query(null, "filter-actioncard").ForEach(x => x.style.display = card is ActionCard ? DisplayStyle.Flex : DisplayStyle.None);
            this.Query(null, "filter-unitcard").ForEach(x => x.style.display = card is UnitCard ? DisplayStyle.Flex : DisplayStyle.None);
            this.Query(null, "filter-itemcard").ForEach(x => x.style.display = card is ItemCard ? DisplayStyle.Flex : DisplayStyle.None);

            this.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
            this.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave());

            if (card is ActionCard acard)
            {
                this.Query<CostIcon>().ForEach(x => x.style.display = DisplayStyle.None);
                var costIcons = this.Query<CostIcon>().ToList();
                var j = 0;
                foreach (var cost in acard.Cost)
                    for (int i = 0; i < cost.Cost; i++)
                    {
                        if (j >= costIcons.Count)
                        {
                            Debug.LogError("Cost too high!");
                            break;
                        }
                        costIcons[j].style.display = DisplayStyle.Flex;
                        costIcons[j++].Set(cost.Class, i);
                    }

                for (; j < costIcons.Count; j++)
                    costIcons[j].style.display = DisplayStyle.None;
            }
            else if (card is UnitCard ucard)
            {
                this.Q<Label>("Life").text = ucard.UnitReference.MaxLife.ToString();
            }
        }
    }
    Card card;

    void OnMouseEnter()
    {
        CardTooltip.Show(Card);
    }

    void OnMouseLeave()
    {
        CardTooltip.Hide();
    }

    public new class UxmlFactory : UxmlFactory<CardItemSmall, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }

}