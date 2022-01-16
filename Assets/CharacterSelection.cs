using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] VisualTreeAsset CardItem;
    [SerializeField] VisualTreeAsset UnitItem;
    int currentDeck;
    [SerializeField] UIDocument uiRoot;
    void Start()
    {
        var selector = uiRoot.rootVisualElement.Q("DeckSelect");
        var player = FindObjectsOfType<PlayerController>().FirstOrDefault(x => x.isLocalPlayer);
        var hub = FindObjectOfType<Hubworld>();
        if (!player || hub == null) return;
        selector.style.display = DisplayStyle.None;
        var charList = selector.Q("Selectors");
        charList.Clear();
        var decks = hub.AvailableDecks;
        foreach (var deck in decks)
        {
            UnitItem.CloneTree(charList, out var idx, out int _);
            charList[idx].Q<UnitItem>().Unit = deck.commanderPrefab;
            charList[idx].Q<UnitItem>().RegisterCallback<MouseDownEvent>(_ => { currentDeck = idx; ShowDeck(); });
            ShowDeck();
        }
        selector.Q<Button>("ChooseDeck").clicked += () =>
        {
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = player.LobbyId, DeckId = currentDeck });
            selector.style.display = hub.TeamSize > JourneyData.Current.SelectedDecks.Count() ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log("Selected Deck " + JourneyData.Current.AvailableDecks[currentDeck].commanderPrefab.Name);
            UpdateSelectedDecks();
        };
        selector.Q<Button>("Close").clicked += () =>
        {
            selector.style.display = DisplayStyle.None;
        };
        UpdateSelectedDecks();
    }

    void UpdateSelectedDecks()
    {
        var decks = uiRoot.rootVisualElement.Q("DeckSelect");
        uiRoot.rootVisualElement.Query<HubPlayerInfo>().ForEach(x => x.UpdateCharacters());
        decks.Q<Button>("ChooseDeck").SetEnabled(!JourneyData.Current.SelectedDecks.Any(x => x.DeckId == currentDeck));
        decks.Query<UnitItem>()
        .ForEach(x =>
        {

            if (JourneyData.Current.SelectedDecks.Any(y => JourneyData.Current.AvailableDecks[y.DeckId].commanderPrefab == x.Unit))
                x.AddToClassList("character-select-taken");
            else
                x.RemoveFromClassList("character-select-taken");
        });
    }

    void ShowDeck()
    {
        Debug.Log("Showing Deck " + currentDeck);
        var cardList = uiRoot.rootVisualElement.Q("DeckSelect").Q<ScrollView>(null, "characterselect-cardlist").contentContainer;
        cardList.Clear();
        var deck = FindObjectOfType<Hubworld>().AvailableDecks[currentDeck];

        foreach (var card in deck.Cards)
        {
            CardItem.CloneTree(cardList, out var idx, out int _);
            cardList[idx].Q<CardItem>().Card = card;
        }

        uiRoot.rootVisualElement.Q("DeckSelect").Q<Button>("ChooseDeck").SetEnabled(!JourneyData.Current.SelectedDecks.Any(x => x.DeckId == currentDeck));
    }
}
