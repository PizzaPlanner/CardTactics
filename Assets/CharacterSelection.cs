using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] Deck[] Decks;
    [SerializeField] VisualTreeAsset CardItem;
    int currentDeck;
    void Start()
    {
        var selector = FindObjectOfType<UIDocument>().rootVisualElement.Q("DeckSelect");
        selector.style.display = Lobby.Current.LocalPlayer.Deck == null ? DisplayStyle.Flex : DisplayStyle.None;
        var tabs = selector.Q("Tabs");
        foreach (var deck in Decks)
        {
            var velm = new VisualElement();
            velm.AddToClassList("decklist");
            tabs.Add(velm);
            foreach (var card in deck.All)
            {
                CardItem.CloneTree(velm, out var idx, out int _);
                velm[idx].Q<CardItem>().Card = card;
                // velm[idx].Q<CardItem>().style.scale = new StyleScale(new Scale(new Vector3(0.7f, 0.7f, 0.7f)));
            }
            selector.Q("ShowNext").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck + 1) % Decks.Length; ShowDeck(); });
            selector.Q("ShowPrevious").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck - 1) % Decks.Length; ShowDeck(); });
            selector.Q<Button>("ChooseDeck").clicked += (() =>
            {
                Lobby.Current.LocalPlayer.Deck = Deck.Create(Decks[currentDeck]);
                selector.style.display = Lobby.Current.LocalPlayer.Deck == null ? DisplayStyle.Flex : DisplayStyle.None;
            });
            ShowDeck();
        }
    }

    void ShowDeck()
    {
        var tabs = FindObjectOfType<UIDocument>().rootVisualElement.Q("Tabs");
        for (int i = 0; i < tabs.childCount; i++)
            tabs[i].style.display = i == currentDeck ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
