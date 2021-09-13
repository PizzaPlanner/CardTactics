using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    public TurnPlate TurnPanel;

    [HideIf("Settings")]
    [SerializeField] GameSettings settingsPrefab;

    [ShowIf("Settings")]
    [InlineEditor]
    public GameSettings Settings;

    // Start is called before the first frame update
    void Awake()
    {
        Settings = GameSettings.Create(settingsPrefab);
        Settings.Map.Create();
        Settings.Map.RevealArea(0);
        foreach (var player in Settings.Players)
        {
            TurnPanel.Register(player);
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

    Skill currentCondition
    {
        get
        {
            var dep = cardSkillProgress - currentCard.Conditions[cardSkillProgress];
            return dep < 0 || dep == cardSkillProgress ? null : currentCard.Skills[dep];
        }
    }

    Card currentCard;
    GameSettings.PlayerInfo currentPlayer;
    int cardSkillProgress;

    [SerializeField] SkillProgressBar skillProgressBar;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && Tile.Hovered != null)
            {
                SelectTile(Tile.Hovered);
            }
            if (Input.GetMouseButtonDown(1) && cardSkillProgress > 0)
            {
                Reset();
                PlayNextSkill(true);
            }
        }
    }

    void SelectTile(Tile tile)
    {
        if (tile.CurrentIndicator == TargetIndicator.Valid || Tile.Hovered.CurrentIndicator == TargetIndicator.Path)
        {
            currentSkill.Preview(tile);
            skillProgressBar.Get(cardSkillProgress).SetProgress(SkillProgressBarItem.Progress.Queued);
            TryNextSkill();
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
            Settings.Map.HideTileSelectionPreview();
        }
    }

    public void Confirm()
    {
        for (int i = 0; i < currentCard.Skills.Length; i++)
        {
            if (skillProgressBar.Get(i).State == SkillProgressBarItem.Progress.Queued)
                currentCard.Skills[i].OnUse();
        }

        currentCard.Play(Settings, currentPlayer);

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
        currentCard.Skip(Settings, currentPlayer);
        Next();
    }

    void Next()
    {
        currentCard = null;
        currentPlayer = null;
        cardSkillProgress = 0;
        Settings.Map.HideTileSelectionPreview();
        skillProgressBar.Clear();
        TurnPanel.Next();
    }

    internal void SetTurn(GameSettings.PlayerInfo player, Card card)
    {
        currentCard = card;
        currentPlayer = player;
        cardSkillProgress = 0;
        foreach (var skill in card.Skills)
            skill.Init(Settings);
        skillProgressBar.SetSkills(card.Skills, TryNextSkill);
        PlayNextSkill();
    }

    void PlayNextSkill(bool tryResetOnFail = false)
    {
        Settings.Map.HideTileSelectionPreview();
        if (currentCard.Skills.Length == 0) return;
        if (!currentSkill?.Activate(currentPlayer, currentCard, currentDependency, currentCondition) ?? false)
        {
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
}
