using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardItem : VisualElement
{
    public Card Card
    {
        get => card;
        set
        {
            card = value;
            if (card != null)
            {
                this.Q("Icon").style.backgroundImage = new StyleBackground(card.Icon);
                this.Query<Label>("Name").ForEach(x => x.text = card.Name);
                this.Query<Label>("Description").ForEach(x => x.text = GetDescription(card));
            }
            else
            {
                this.Q("Icon").style.backgroundImage = new StyleBackground();
            }

            this.Query(null, "filter-actioncard").ForEach(x => x.style.display = DisplayStyle.None);
            this.Query(null, "filter-unitcard").ForEach(x => x.style.display = DisplayStyle.None);
            this.Query(null, "filter-itemcard").ForEach(x => x.style.display = DisplayStyle.None);

            this.Query(null, "filter-actioncard").ForEach(x => { if (card is ActionCard) x.style.display = DisplayStyle.Flex; });
            this.Query(null, "filter-unitcard").ForEach(x => { if (card is UnitCard) x.style.display = DisplayStyle.Flex; });
            this.Query(null, "filter-itemcard").ForEach(x => { if (card is ItemCard) x.style.display = DisplayStyle.Flex; });

            if (card is ActionCard acard)
            {
                this.Query<ResourcePanel>().ForEach(x => x.Resources = acard.Cost);
            }
            else if (card is UnitCard ucard)
            {
                this.Query<Label>("Life").ForEach(x => x.text = ucard.UnitReference.MaxLife.ToString());
                this.Query<Label>("Speed").ForEach(x => x.text = ucard.UnitReference.Speed.ToString());
                this.Query<ResourcePanel>().ForEach(x => x.Resources = ucard.UnitReference.Resources);
            }
        }
    }
    Card card;

    public CardItem()
    {
        this.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter?.Invoke());
        this.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave?.Invoke());
    }

    public Action OnMouseEnter, OnMouseLeave;

    static string GetDescription(Card card)
    {
        return card.GetFormattedDescription();
    }


    public new class UxmlFactory : UxmlFactory<CardItem, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }

}