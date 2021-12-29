using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitStatusBar : VisualElement
{
    VisualElement bar;
    Label life;

    public Unit Unit
    {
        get => unit; set
        {
            unit = value;
            this.Q("BuffBar").style.display = unit.TryGetComponent(out deck) ? DisplayStyle.Flex : DisplayStyle.None;
            bar = this.Q("Lifebar-Bar");
            life = this.Q<Label>("Life");

            this.Q("BuffBar").Clear();

            UpdateValues();
        }
    }
    Unit unit;
    public bool FollowUnit;
    IngameDeck deck;

    public VisualTreeAsset CardPrefab;
    float lastBuffUpdate;

    public void UpdateValues()
    {
        if (FollowUnit)
        {
            style.position = Position.Absolute;
            Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(panel, unit.transform.position, Camera.main);
            transform.position = newPosition - new Vector2(layout.width / 2, 0);
            bar.style.width = new StyleLength(new Length(unit.DisplayLife / unit.MaxLife * 100, LengthUnit.Percent));
        }

        life.text = unit.DisplayLife.ToString("N0");

        // Update Buffbar. Only once per 0.5s if FollowUnit is true
        if (deck == null || (FollowUnit && Time.timeSinceLevelLoad - lastBuffUpdate < 0.5f))
            return;

        lastBuffUpdate = Time.timeSinceLevelLoad;

        var hasNot = deck.Unplayable.ToArray();
        foreach (var item in this.Query<CardItem>().ToList())
        {
            bool hasCard = false;
            for (int i = 0; i < hasNot.Length; i++)
            {
                if (hasNot[i] != null && item.Card == hasNot[i].Target)
                {
                    hasNot[i] = null;
                    hasCard = true;
                    break;
                }
            }
            if (!hasCard)
                item.RemoveFromHierarchy();
        }

        foreach (var ctx in hasNot)
            if (ctx != null)
            {
                CardPrefab.CloneTree(this.Q("BuffBar"), out var idx, out var _);
                this.Q("BuffBar")[idx].Q<CardItem>().Context = ctx;
            }
    }

    public new class UxmlFactory : UxmlFactory<UnitStatusBar, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }
}
