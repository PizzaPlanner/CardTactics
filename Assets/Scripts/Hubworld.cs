using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public interface IJourneyProvider
{
    Deck[] StartingDecks { get; }
}

public class Hubworld : MonoBehaviour, IJourneyProvider
{

    [System.Serializable]
    class MapWithRequirements
    {
        public Map MapPrefab;
        public string[] RequiredAchievements;
        public string[] PreventingAchievements;
        public bool Playable => JourneyData.Current.SelectedDecks.Count >= MapPrefab.MinPartySize && JourneyData.Current.SelectedDecks.Count <= MapPrefab.MaxPartySize;
    }

    [SerializeField] public int TeamSize;
    [SerializeField] MapWithRequirements[] Maps;
    [SerializeField] public Deck[] AvailableDecks;
    [SerializeField] VisualTreeAsset UnitItem;
    [SerializeField] VisualTreeAsset PlayerInfo;

    Label SelectedMapNameText;
    Label SelectedMapTeamSize;
    Button PlaySelectedMap;
    VisualElement PlayerInfoPanel;

    public Deck[] StartingDecks => AvailableDecks;
    int SelectedMapId = -1;

    public void SelectMap(int id)
    {
        if (id >= Maps.Length)
            SelectedMapId = -1;
        else
            SelectedMapId = id;
    }

    void Start()
    {
        var root = GetComponentInChildren<UIDocument>().rootVisualElement;
        var planningPanel = root.Q("PlanningPanel");
        PlaySelectedMap = planningPanel.Q<Button>("Go");
        PlaySelectedMap.clicked += PlayMap;
        SelectedMapNameText = planningPanel.Q<Label>("MapName");
        SelectedMapTeamSize = planningPanel.Q<Label>("CharacterCount");

        PlayerInfoPanel = root.Q("PlayerInfo");
        PlayerInfoPanel.Clear();
        var deckSelect = root.Q("DeckSelect");
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            PlayerInfo.CloneTree(PlayerInfoPanel, out var idx, out var _);
            var item = PlayerInfoPanel[idx].Q<HubPlayerInfo>();
            item.Init(UnitItem, root.Q("DeckSelect"));
            item.Target = player;
        }
    }

    public void PlayMap()
    {
        var prefab = Maps[SelectedMapId].MapPrefab;
        LoadingScreen.PlayMap(prefab);
    }

    void Update()
    {
        if (SelectedMapId >= 0)
        {
            SelectedMapNameText.text = Maps[SelectedMapId].MapPrefab.name;
            SelectedMapTeamSize.text = $"Team size: {JourneyData.Current.SelectedDecks.Count}\nAllowed: {Maps[SelectedMapId].MapPrefab.MinPartySize}-{Maps[SelectedMapId].MapPrefab.MaxPartySize}";
        }
        SelectedMapNameText.style.display = SelectedMapId < 0 ? DisplayStyle.None : DisplayStyle.Flex;
        SelectedMapTeamSize.style.display = SelectedMapId < 0 ? DisplayStyle.None : DisplayStyle.Flex;
        PlaySelectedMap.style.display = SelectedMapId < 0 || !Maps[SelectedMapId].Playable ? DisplayStyle.None : DisplayStyle.Flex;
    }
}