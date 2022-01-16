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
            if (!JourneyData.PrepareNew(TutorialDecks))
            {
                Debug.LogWarning("Only Host can start new Journeys");
                return;
            }
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = 0, DeckId = 0 });
            LoadingScreen.PlayMap(Tutorial);
        };
        root.Q<Button>("Campaign").clicked += () =>
        {
            if (!JourneyData.PrepareNew(Campaign.StartingDecks))
            {
                Debug.LogWarning("Only Host can start new Journeys");
                return;
            };
            LoadingScreen.LoadHub(Campaign);
        };
        root.Q<Button>("Quickmatch").clicked += () =>
        {
            if (!JourneyData.PrepareNew(QuickmatchDecks))
            {
                Debug.LogWarning("Only Host can start new Journeys");
                return;
            };
            JourneyData.Current.SelectedDecks.Add(new JourneyData.PlayerWithDeck { PlayerId = 0, DeckId = 0 });
            LoadingScreen.PlayMap(Quickmatch);
        };
    }
}