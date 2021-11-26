using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] Deck[] Decks;
    [SerializeField] VisualTreeAsset CardItem;
    int currentDeck;
    [SerializeField] UIDocument uiRoot;
    void Start()
    {
        var selector = uiRoot.rootVisualElement.Q("DeckSelect");
        var player = FindObjectsOfType<PlayerController>().FirstOrDefault(x => x.isLocalPlayer);
        if (!player) return;
        selector.style.display = player.Deck == null ? DisplayStyle.Flex : DisplayStyle.None;
        var tabs = selector.Q("Tabs");
        foreach (var deck in Decks)
        {
            var velm = new VisualElement();
            velm.AddToClassList("decklist");
            tabs.Add(velm);
            foreach (var card in deck.Cards)
            {
                CardItem.CloneTree(velm, out var idx, out int _);
                velm[idx].Q<CardItem>().Card = card;
                // velm[idx].Q<CardItem>().style.scale = new StyleScale(new Scale(new Vector3(0.7f, 0.7f, 0.7f)));
            }
            selector.Q("ShowNext").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck + 1) % Decks.Length; ShowDeck(); });
            selector.Q("ShowPrevious").RegisterCallback<MouseDownEvent>(_ => { currentDeck = (currentDeck - 1) % Decks.Length; ShowDeck(); });
            selector.Q<Button>("ChooseDeck").clicked += (() =>
            {
                player.SelectDeck(Decks[currentDeck]);
                selector.style.display = player.Deck == null ? DisplayStyle.Flex : DisplayStyle.None;
            });
            ShowDeck();
        }
    }

    void ShowDeck()
    {
        var tabs = uiRoot.rootVisualElement.Q("Tabs");
        for (int i = 0; i < tabs.childCount; i++)
            tabs[i].style.display = i == currentDeck ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
