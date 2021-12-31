using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{
    [SerializeField] Map Tutorial;
    [SerializeField] Deck[] TutorialDecks;
    [SerializeField] Map Quickmatch;
    [SerializeField] Deck[] QuickmatchDecks;
    [SerializeField] Hubworld Campaign;
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("Tutorial").clicked += () =>
        {
            JourneyData.PrepareNew(TutorialDecks);
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = 0, DeckId = 0 });
            LoadingScreen.PlayMap(Tutorial);
        };
        root.Q<Button>("Campaign").clicked += () => { JourneyData.PrepareNew(Campaign.StartingDecks); LoadingScreen.LoadHub(Campaign); };
        root.Q<Button>("Quickmatch").clicked += () =>
        {
            JourneyData.PrepareNew(QuickmatchDecks);
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = 0, DeckId = 0 });
            LoadingScreen.PlayMap(Quickmatch);
        };
    }
}