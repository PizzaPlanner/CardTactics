using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] VisualTreeAsset CardItem;
    int currentDeck;
    [SerializeField] UIDocument uiRoot;
    void Start()
    {
        var selector = uiRoot.rootVisualElement.Q("DeckSelect");
        var player = FindObjectsOfType<PlayerController>().FirstOrDefault(x => x.isLocalPlayer);
        var hub = FindObjectOfType<Hubworld>();
        if (!player || hub == null) return;
        selector.style.display = hub.TeamSize > JourneyData.Current.SelectedDecks.Count() ? DisplayStyle.Flex : DisplayStyle.None;
        var tabs = selector.Q("Tabs");
        var decks = hub.AvailableDecks;
        foreach (var deck in decks)
        {
            var velm = new VisualElement();
            velm.AddToClassList("decklist");
            tabs.Add(velm);
            foreach (var card in deck.Cards)
            {
                CardItem.CloneTree(velm, out var idx, out int _);
                velm[idx].Q<CardItem>().Card = card;
            }
            selector.Q("ShowNext").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck + 1) % decks.Length; ShowDeck(); });
            selector.Q("ShowPrevious").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck - 1) % decks.Length; ShowDeck(); });
            ShowDeck();
        }
        selector.Q<Button>("ChooseDeck").clicked += (() =>
        {
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = player.LobbyId, DeckId = currentDeck });
            selector.style.display = hub.TeamSize > JourneyData.Current.SelectedDecks.Count() ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log("Selected Deck " + JourneyData.Current.AvailableDecks[currentDeck].commanderPrefab.Name);
        });
    }

    void ShowDeck()
    {
        var tabs = uiRoot.rootVisualElement.Q("Tabs");
        for (int i = 0; i < tabs.childCount; i++)
            tabs[i].style.display = i == currentDeck ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
