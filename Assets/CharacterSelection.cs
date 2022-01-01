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
        selector.style.display = hub.TeamSize > JourneyData.Current.SelectedDecks.Count() ? DisplayStyle.Flex : DisplayStyle.None;
        var charList = selector.Q("Selectors").Q<ScrollView>().contentContainer;
        charList.Clear();
        var decks = hub.AvailableDecks;
        foreach (var deck in decks)
        {
            UnitItem.CloneTree(charList, out var idx, out int _);
            charList[idx].Q<UnitItem>().Unit = deck.commanderPrefab;
            charList[idx].Q<UnitItem>().RegisterCallback<MouseDownEvent>(_ => { currentDeck = idx; ShowDeck(); });
            ShowDeck();
        }
        selector.Q<Button>("ChooseDeck").clicked += (() =>
        {
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = player.LobbyId, DeckId = currentDeck });
            selector.style.display = hub.TeamSize > JourneyData.Current.SelectedDecks.Count() ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log("Selected Deck " + JourneyData.Current.AvailableDecks[currentDeck].commanderPrefab.Name);
            UpdateSelectedDecks();
        });
        UpdateSelectedDecks();
    }

    void UpdateSelectedDecks()
    {
        var root = uiRoot.rootVisualElement.Q("PlayerInfo");
        root.Clear();
        Dictionary<int, List<Deck>> rearrangedDecks = new();
        foreach (var deck in JourneyData.Current.SelectedDecks)
        {
            if (!rearrangedDecks.ContainsKey(deck.PlayerId))
                rearrangedDecks[deck.PlayerId] = new();
            rearrangedDecks[deck.PlayerId].Add(JourneyData.Current.AvailableDecks[deck.DeckId]);
        }

        foreach (var player in rearrangedDecks.Keys)
        {
            var velm = new VisualElement();
            var label = new Label { text = "Player " + player };
            label.AddToClassList("color-default-text");
            velm.Add(label);
            foreach (var deck in rearrangedDecks[player])
            {
                UnitItem.CloneTree(velm, out var idx, out int _);
                velm[idx].Q<UnitItem>().Unit = deck.commanderPrefab;
            }
            root.Add(velm);
        }
    }

    void ShowDeck()
    {
        Debug.Log("Showing Deck " + currentDeck);
        var cardList = uiRoot.rootVisualElement.Q("DeckSelect").Q(null, "decklist");
        cardList.Clear();
        var deck = FindObjectOfType<Hubworld>().AvailableDecks[currentDeck];

        foreach (var card in deck.Cards)
        {
            CardItem.CloneTree(cardList, out var idx, out int _);
            cardList[idx].Q<CardItem>().Card = card;
        }
    }
}
