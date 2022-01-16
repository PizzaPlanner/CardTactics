using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HubPlayerInfo : VisualElement
{
    VisualTreeAsset UnitItem;

    PlayerController target;
    public PlayerController Target
    {
        get => target;
        set
        {
            target = value;
            this.Q<Label>().text = target.name;
            UpdateCharacters();
        }
    }

    public void Init(VisualTreeAsset unitAsset, VisualElement characterSelectionPanel)
    {
        UnitItem = unitAsset;
        this.Q<Button>("Add").clicked += () =>
        {
            characterSelectionPanel.style.display = DisplayStyle.Flex;
        };
    }

    public HubPlayerInfo() { }

    internal void UpdateCharacters()
    {
        var list = this.Q("characters");
        list.Clear();
        foreach (var deck in JourneyData.Current.SelectedDecks)
        {
            if (deck.PlayerId != Target.LobbyId) return;

            UnitItem.CloneTree(list, out var idx, out var _);
            var item = list[idx];
            item.Q<UnitItem>().Unit = JourneyData.Current.AvailableDecks[deck.DeckId].commanderPrefab;
            item.Q<UnitItem>().RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.button == 1)
                {
                    JourneyData.Current.SelectedDecks.Remove(deck);
                    UpdateCharacters();
                }
            });
        }
    }

    public new class UxmlFactory : UxmlFactory<HubPlayerInfo, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
    }
}