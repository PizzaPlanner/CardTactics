using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TurnType
{
    Preparation,
    ExecuteRound,
    PlaceUnit,
    PrepareTurn,
}

public class Board : MonoBehaviour
{
    public GameObject TilePrefab;
    public int Width, Height;

    public int UnitsPerTeam;
    public Deck[] Decks;

    public Tile[] TilePrefabs;

    public Tile[,] Tiles;

    public TurnPlate TurnPanel;

    static Board instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        Tiles = new Tile[Width, Height];
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                var tile = Instantiate(TilePrefabs[Random.Range(0, TilePrefabs.Length)], new Vector3(w - Width / 2, 0, h - Height / 2), Quaternion.identity).GetComponent<Tile>();
                Tiles[w, h] = tile;
                tile.Position = (w, h);
                tile.Neighbours = new List<Tile>();
                if (w > 0)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w - 1, h]);
                    Tiles[w - 1, h].Neighbours.Add(Tiles[w, h]);
                }
                if (h > 0)
                {
                    Tiles[w, h].Neighbours.Add(Tiles[w, h - 1]);
                    Tiles[w, h - 1].Neighbours.Add(Tiles[w, h]);
                }
            }
        }

        foreach (var deck in Decks)
        {
            deck.Initialize();
            deck.Show();
            TurnPanel.RegisterPlayer(deck.Player);
        }
        TurnPanel.StartTurn();
    }

    void ResetSelection()
    {
        selection = null;
        // ShowOptions(null);
    }

    Tile selection;
    Skill currentSkill => currentCard.Skills[cardSkillProgress];
    Card currentCard;
    int currentPlayer;
    int cardSkillProgress;

    [SerializeField] Button confirmButton;
    [SerializeField] RectTransform skillDisplay;
    [SerializeField] Image skillDisplayPrefab;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                selection = Tile.Hovered;
                if (selection.CurrentIndicator == TargetIndicator.Valid || selection.CurrentIndicator == TargetIndicator.Path)
                {
                    currentSkill.Preview(selection);
                    skillDisplay.GetChild(cardSkillProgress).GetComponent<Image>().color = Color.green;
                    cardSkillProgress++;
                    HideTileSelectionPreview();
                    if (cardSkillProgress >= currentCard.Skills.Length)
                    {
                        confirmButton.interactable = true;
                    }
                    else
                    {
                        PlayNextSkill();
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                Reset();
                PlayNextSkill();
            }
        }
    }

    public void Confirm()
    {
        foreach (var skill in currentCard.Skills)
            skill.OnUse();
        Next();
    }

    public void Reset()
    {
        if (cardSkillProgress <= 0) return;
        cardSkillProgress--;
        var previousTile = currentCard?.Skills[cardSkillProgress].Reset();
        skillDisplay.GetChild(cardSkillProgress).GetComponent<Image>().color = Color.white;
        confirmButton.interactable = false;
        Debug.Log("Previous tile was: " + previousTile);
        selection = previousTile;
    }

    public void Skip()
    {
        while (cardSkillProgress >= 0)
            Reset();
        Next();
    }

    void Next()
    {
        currentCard = null;
        ResetSelection();
        HideTileSelectionPreview();
        foreach (Transform child in skillDisplay)
            GameObject.Destroy(child.gameObject);
        TurnPanel.Next();
    }

    internal void SetTurn(int player, Card card)
    {
        Debug.Log("New Turn with Card " + card.Name);
        currentCard = card;
        currentPlayer = player;
        cardSkillProgress = 0;
        confirmButton.interactable = false;
        foreach (var skill in currentCard.Skills)
        {
            var icon = GameObject.Instantiate(skillDisplayPrefab);
            icon.sprite = skill.Icon;
            icon.transform.SetParent(skillDisplay);
        }
        PlayNextSkill();
    }

    void PlayNextSkill()
    {
        Debug.Log("Selection is " + selection);
        if (!currentSkill?.Activate(currentPlayer, currentCard, selection) ?? false)
            Debug.LogError("Failed to activate Skill #" + currentSkill);
    }

    public void ShowPlacementTiles(TargetType filter)
    {
        foreach (var tile in Tiles)
        {
            TileMatches(tile, filter);
        }
    }

    public void HideTileSelectionPreview()
    {
        foreach (var tile in Tiles)
        {
            tile.SetIndicator(TargetIndicator.Hidden);
            tile.ClearPathingInfo();
        }
    }

    public bool TileMatches(Tile tile, TargetType filter)
    {
        var validTarget = ((filter & TargetType.Empty) > 0) && (tile.Unit == null);
        if (tile.Unit != null)
        {
            validTarget |= ((filter & TargetType.Obstacle) > 0) && (tile.Unit.Player == 0);
            validTarget |= ((filter & TargetType.Ally) > 0) && (GetTeam(tile.Unit.Player) == GetTeam(currentPlayer)) && (tile.Unit.Player != 0);
            validTarget |= ((filter & TargetType.Enemy) > 0) && (GetTeam(tile.Unit.Player) != GetTeam(currentPlayer)) && (tile.Unit.Player != 0);
        }

        tile.SetIndicator(validTarget ? TargetIndicator.Valid : TargetIndicator.Invalid);
        return validTarget;
    }

    public void ShowRange(Tile origin, int minRange, int maxRange, TargetType filter, RangeType type)
    {
        List<Tile> rangeTiles = new List<Tile>();
        foreach (var tile in Tiles)
        {
            // Check range
            if (Tile.Distance(origin, tile) > maxRange || Tile.Distance(origin, tile) < minRange)
            {
                tile.SetIndicator(TargetIndicator.Hidden);
                continue;
            }

            // Check TargetType

            if (TileMatches(tile, filter))
                rangeTiles.Add(tile);
        }
        if (type == RangeType.Pathfinding)
        {
            foreach (var tile in rangeTiles)
            {
                if (!Tile.FindPath(origin, tile, maxRange, rangeTiles))
                    tile.SetIndicator(TargetIndicator.Hidden);
            }
        }
    }

    public static int GetTeam(int player) => player % 2;

    public static Color PlayerColor(int player)
    {
        switch (player)
        {
            case 1: return Color.red;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.green;
            default: return Color.white;
        }
    }
}
