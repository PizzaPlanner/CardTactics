
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Card : ScriptableObject, IPlayable
{
    public enum CardState
    {
        Unplayed,
        Queued,
        Current,
        Active,
        Destroyed
    }

    [ShowInInspector]
    [HideInEditorMode]
    public CardState State { get; protected set; }

    public IEnumerator Play(Board board, Map map, Unit unit, PlayerInfo player)
    {
        State = CardState.Current;
        CardTooltip.Show(this);
        yield return new WaitForSeconds(board.CardPlayPreTime);
        yield return PlaySkills(map, unit, player, true);
        yield return new WaitForSeconds(board.CardPlayPostTime);
        CardTooltip.Hide();
        State = StateOnPlayed;
        player.Deck.MarkPlayed(this);
    }

    public void Destroy() { State = CardState.Destroyed; }

    public void SetSelectionState(bool value)
    {
        State = value ? CardState.Queued : CardState.Unplayed;
    }

    public virtual List<Card> GetRelatedCards()
    {
        var list = new List<Card>();
        if (Source != null)
            list.Add(Source);
        foreach (var skill in Skills)
        {
            list.AddRange(skill.GetRelatedCards());
        }
        return list;
    }

    public virtual void OnTurnStart(Map map, PlayerInfo player)
    {
        foreach (var skill in Skills)
        {
            skill.Prepare();
            if (State == CardState.Active && skill is Passive passive)
                passive.OnRoundStart(map, player);
        }
    }

    public virtual void OnTurnEnd(Map map, PlayerInfo player) { }

    protected abstract CardState StateOnPlayed { get; }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }

    [Required]
    public CardClass Class;
    public Card Source { get; private set; }

    protected abstract void OnInit();
    public void Init(Card source = null, CardState initialState = CardState.Unplayed)
    {
        State = initialState;
        Source = source;
        for (int i = 0; i < Skills2.Length; i++)
        {
            Skills2[i].Skill.Source = this;
        }
        OnInit();
    }

    public SkillInfo[] Skills2;

    public IEnumerable<Skill> Skills => Skills2.Select(x => x.Skill);

    public void ExtendWith(SkillInfo newSkill, int index)
    {
        var listS = Skills2.ToList();
        listS.Insert(index, newSkill);
        Skills2 = listS.ToArray();
    }

    public Tile SkillTarget { get; set; }
    public bool WantSkillSkip { get; set; }
    public bool WantComplete { get; set; }
    public bool WantReset { get; set; }

    protected IEnumerator PlaySkills(Map map, Unit unit, PlayerInfo player, bool autocompleteIfPossible)
    {
        if (Skills2.Length == 0)
            WantComplete = true;
        for (int i = 0; i <= Skills2.Length; i++)
        {
            if (i < Skills2.Length)
            {
                SkillTarget = null;
                WantSkillSkip = false;
                WantComplete = autocompleteIfPossible;
                WantReset = false;
                if (Skills2[i].CanUse(map.TileOf(unit), Skills2.ToList().GetRange(0, i), out var inputTile)
                && Skills2[i].Skill.Activate(map, player, this, inputTile))
                {
                    if (!player.IsAI && SkillTarget == null)
                        map.ShowTileIndicators();

                    yield return new WaitUntil(() => SkillTarget != null || WantReset || (WantSkillSkip && Skills2[i].Skill.IsOptional));

                    if (SkillTarget != null)
                    {
                        yield return Skills2[i].Skill.Preview(SkillTarget);
                        if (player.IsAI)
                            WantComplete = true;
                    }
                    else if (WantSkillSkip)
                    {
                        Skills2[i].Skill.Skip();
                    }
                    else if (WantReset)
                    {
                        i = PartialReset();
                    }
                }
                map.HideTileSelectionPreview();
            }
            else
            {
                yield return new WaitUntil(() => WantComplete || WantReset);
                if (WantComplete)
                {
                    foreach (var skill in Skills)
                    {
                        if (skill.PlayProgress == Skill.Progress.Queued)
                            skill.Use();
                    }
                }
                else if (WantReset)
                {
                    i = PartialReset();
                }
            }
        }
    }

    int PartialReset()
    {
        for (int j = Skills2.Length - 1; j >= 0; j--)
        {
            var prog = Skills2[j].Skill.PlayProgress;
            Skills2[j].Skill.Reset();
            Debug.Log("Resetting skill " + j);
            if (prog == Skill.Progress.Queued ||
                prog == Skill.Progress.Skipped)
            {
                return j - 1;
            }
        }
        return -1;
    }
}