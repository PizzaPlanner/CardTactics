using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public GameObject TilePrefab;
    public int Width, Height;

    public Deck[] Decks;

    public Tile[] TilePrefabs;

    public Tile[,] Tiles;

    public TurnPlate TurnPanel;

    // Start is called before the first frame update
    void Awake()
    {
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
            TurnPanel.Register(deck);
        }
        TurnPanel.StartTurn();
    }

    Skill currentSkill => currentCard.Skills[cardSkillProgress];
    Skill currentDependency
    {
        get
        {
            var dep = cardSkillProgress - currentCard.Dependencies[cardSkillProgress];
            return dep < 0 || dep == cardSkillProgress ? null : currentCard.Skills[dep];
        }
    }

    Card currentCard;
    int currentPlayer;
    int cardSkillProgress;

    [SerializeField] SkillProgressBar skillProgressBar;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Tile.Hovered == null) return;
                if (Tile.Hovered.CurrentIndicator == TargetIndicator.Valid || Tile.Hovered.CurrentIndicator == TargetIndicator.Path)
                {
                    currentSkill.Preview(Tile.Hovered);
                    skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Queued);
                    TryNextSkill();
                }
            }
            if (Input.GetMouseButtonDown(1) && cardSkillProgress > 0)
            {
                Reset();
                PlayNextSkill(true);
            }
        }
    }

    private void TryNextSkill()
    {
        cardSkillProgress++;
        if (cardSkillProgress < currentCard.Skills.Length)
        {
            PlayNextSkill();
        }
        else
        {
            HideTileSelectionPreview();
        }
    }

    public void Confirm()
    {
        for (int i = 0; i < currentCard.Skills.Length; i++)
        {
            if (skillProgressBar.Get(i).State == SkillProgressBarItem.Progress.Queued)
                currentCard.Skills[i].OnUse();
        }

        Next();
    }

    public void Reset()
    {
        if (cardSkillProgress <= 0) return;
        if (cardSkillProgress < currentCard.Skills.Length)
            skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Pending);
        cardSkillProgress--;
        currentCard?.Skills[cardSkillProgress].Reset();
        skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Pending);
    }

    public void Skip()
    {
        while (cardSkillProgress > 0)
            Reset();
        Next();
    }

    void Next()
    {
        currentCard = null;
        HideTileSelectionPreview();
        skillProgressBar.Clear();
        TurnPanel.Next();
    }

    internal void SetTurn(int player, Card card)
    {
        Debug.Log("New Turn with Card " + card.Name);
        currentCard = card;
        currentPlayer = player;
        cardSkillProgress = 0;
        foreach (var skill in card.Skills)
            skill.Init(card);
        skillProgressBar.SetSkills(card.Skills, TryNextSkill);
        PlayNextSkill();
    }

    void PlayNextSkill(bool tryResetOnFail = false)
    {
        HideTileSelectionPreview();
        if (!currentSkill?.Activate(currentPlayer, currentCard, currentDependency) ?? false)
        {
            Debug.LogError("Failed to activate Skill #" + currentSkill);
            if (tryResetOnFail && cardSkillProgress > 0 && (
                skillProgressBar.Get(cardSkillProgress - 1).State == SkillProgressBarItem.Progress.Skipped ||
                skillProgressBar.Get(cardSkillProgress - 1).State == SkillProgressBarItem.Progress.Queued ||
                skillProgressBar.Get(cardSkillProgress - 1).State == SkillProgressBarItem.Progress.Failed
                ))
            {
                Reset();
                PlayNextSkill(true);
                return;
            }

            skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Failed);
            if (currentSkill.IsOptional)
                TryNextSkill();
        }
        else
        {
            skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Now);
        }
    }

    public int ShowPlacementTiles(TargetType filter)
    {
        int count = 0;
        foreach (var tile in Tiles)
        {
            count += TileMatches(tile, filter) ? 1 : 0;
        }
        return count;
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

    public int ShowRange(Tile origin, int minRange, int maxRange, TargetType filter, RangeType type)
    {
        int matches = 0;
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
            {
                matches++;
                rangeTiles.Add(tile);
            }
        }
        if (type == RangeType.Pathfinding)
        {
            foreach (var tile in rangeTiles)
            {
                if (!Tile.FindPath(origin, tile, maxRange, rangeTiles))
                {
                    tile.SetIndicator(TargetIndicator.Hidden);
                    matches--;
                }
            }
        }
        return matches;
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
