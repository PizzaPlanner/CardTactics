using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    public TurnPlate TurnPanel;

    [HideIf("Settings")]

    [ShowIf("Settings")]
    [InlineEditor]
    public GameSettings Settings;

    public bool Paused;

    // Start is called before the first frame update
    public void Initialize(GameSettings settingsPrefab)
    {
        Settings = GameSettings.Create(settingsPrefab);
        Settings.Map.Create();
        Settings.Map.RevealArea(0);
        Paused = false;
        foreach (var player in Settings.Players)
        {
            if (!player.RegisterLater)
            {
                TurnPanel.Register(player);
            }
        }
    }

    public void StartGame()
    {
        TurnPanel.StartTurn();
    }

    void Start()
    {
        if (Settings == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
    }

    Card currentCard;

    [SerializeField] SkillProgressBar skillProgressBar;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && currentCard != null)
        {
            if (Input.GetMouseButtonDown(0) && Tile.Hovered != null)
            {
                if (Tile.Hovered.CurrentIndicator == TargetIndicator.Valid || Tile.Hovered.CurrentIndicator == TargetIndicator.Path)
                    currentCard.SkillTarget = Tile.Hovered;
            }
            if (Input.GetMouseButtonDown(1))
            {
                currentCard.WantReset = true;
            }
        }
    }

    public void Confirm()
    {
        currentCard.WantComplete = true;
    }

    public void Skip()
    {
        currentCard.WantCardSkip = true;
    }

    void Next()
    {
        currentCard = null;
        Settings.Map.HideTileSelectionPreview();
        skillProgressBar.Clear();
        TurnPanel.Next();
    }

    internal void SetTurn(GameSettings.PlayerInfo player, Card card)
    {
        currentCard = card;
        skillProgressBar.SetSkills(card, player, () => currentCard.WantSkillSkip = true, card is not UnitCard);
        var cr = StartCoroutine(currentCard.Play(Settings, player, onComplete: Next));
    }
}
