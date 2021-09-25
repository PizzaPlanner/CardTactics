using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    public TurnPlate TurnPanel;

    [HideIf("Settings")]

    [ShowIf("Settings")]
    [InlineEditor]
    public Map Map;

    // Start is called before the first frame update
    public void Initialize(Map mapPrefab)
    {
        Map = Map.Create(mapPrefab);
        Map.RevealArea(0);
        foreach (var player in Map.Players)
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
        if (Map == null && GameObject.FindObjectOfType<LoadingScreen>() == null)
            LoadingScreen.GoToMenu();
    }

    Card currentCard;

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

    void Next()
    {
        currentCard = null;
        Map.HideTileSelectionPreview();
        TurnPanel.Next();
    }

    internal void SetTurn(PlayerInfo player, Card card)
    {
        currentCard = card;
        var cr = StartCoroutine(currentCard.Play(Map, player, onComplete: Next));
    }
}
