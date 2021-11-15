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
            this.Q("Icon").style.backgroundImage = new StyleBackground(card.Icon);
            this.Query<Label>("Name").ForEach(x => x.text = card.Name);
            this.Query<Label>("Description").ForEach(x => x.text = GetDescription(card));

            this.Query(null, "filter-actioncard").ForEach(x => x.style.display = card is ActionCard ? DisplayStyle.Flex : DisplayStyle.None);
            this.Query(null, "filter-unitcard").ForEach(x => x.style.display = card is UnitCard ? DisplayStyle.Flex : DisplayStyle.None);
            this.Query(null, "filter-itemcard").ForEach(x => x.style.display = card is ItemCard ? DisplayStyle.Flex : DisplayStyle.None);

            this.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
            this.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave());

            if (card is ActionCard acard)
            {
                this.Query<ResourcePanel>().ForEach(x => x.Resources = acard.Cost);
            }
            else if (card is UnitCard ucard)
            {
                this.Query<Label>("Life").ForEach(x => x.text = ucard.UnitReference.MaxLife.ToString());
                this.Query<Label>("Speed").ForEach(x => x.text = ucard.UnitReference.Speed.ToString());
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

    static string GetDescription(Card card)
    {
        var descriptions = new List<string>();
        for (int i = 0; i < card.Skills2.Length; i++)
        {
            if (card.Skills2[i].Skill.Description != null)
                descriptions.Add(card.Skills2[i].Skill.Description);
        }
        return string.Join("\n", descriptions);
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