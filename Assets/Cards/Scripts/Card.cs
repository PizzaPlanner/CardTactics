
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Card : ScriptableObject, IPlayable
{
    public enum CardState
    {
        Unplayed,
        Played,
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
        OnPlay(board, map, unit, player);
        yield return new WaitForSeconds(board.CardPlayPreTime);
        yield return PlaySkills(map, unit, player, true);
        yield return new WaitForSeconds(board.CardPlayPostTime);
        CardTooltip.Hide();
        if (Skills.Count(x => x is Passive) > 0)
            State = CardState.Active;
        else
            State = StateOnPlayed;
    }

    public void Destroy() { State = CardState.Destroyed; }

    protected virtual void OnPlay(Board board, Map map, Unit unit, PlayerInfo player) { }

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

    public virtual void OnRoundStart(Map map, PlayerInfo player)
    {
        foreach (var skill in Skills)
        {
            skill.Prepare();
            if (State == CardState.Active && skill is Passive passive)
            {
                Debug.Log("Triggering Passive with RL: " + passive.RoundsLeft);
                if (passive.RoundsLeft >= 0 && passive.RoundsLeft-- == 0)
                {
                    passive.OnPassiveEnd();
                    State = StateOnPlayed;
                }
                else
                    passive.OnRoundStart();
            }
        }
    }

    public virtual void OnRoundEnd(Map map, PlayerInfo player)
    {
        if (State == CardState.Played)
            State = CardState.Unplayed;
    }

    protected abstract CardState StateOnPlayed { get; }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public Card Source { get; private set; }

    protected virtual void OnInit() { }
    public void Init(Card source = null, CardState initialState = CardState.Unplayed)
    {
        State = initialState;
        Source = source;
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].Skill.Source = this;
        }
        OnInit();
    }

    [FormerlySerializedAs("Skills2")]
    [SerializeField]
    SkillInfo[] skills;

    public IEnumerable<Skill> Skills => skills?.Select(x => x.Skill) ?? new Skill[0];

    public void ExtendWith(SkillInfo newSkill, int index)
    {
        var listS = skills.ToList();
        listS.Insert(index, newSkill);
        skills = listS.ToArray();
    }

    public ITilePattern SkillTarget { get; set; }
    public bool WantSkillSkip { get; set; }
    public bool WantComplete { get; set; }
    public bool WantReset { get; set; }

    protected IEnumerator PlaySkills(Map map, Unit unit, PlayerInfo player, bool autocompleteIfPossible)
    {
        if (skills.Length == 0)
            WantComplete = true;
        for (int i = 0; i <= skills.Length; i++)
        {
            if (i < skills.Length)
            {
                SkillTarget = null;
                WantSkillSkip = false;
                WantComplete = autocompleteIfPossible;
                WantReset = false;
                if (skills[i].CanUse(map.TileOf(unit), skills.ToList().GetRange(0, i), out var inputTile)
                && skills[i].Skill.Activate(map, player, this, inputTile, out var validPatterns))
                {
                    if (!player.IsAI && SkillTarget == null)
                        map.ShowTileIndicators(validPatterns);

                    yield return new WaitUntil(() => SkillTarget != null || WantReset || (WantSkillSkip && skills[i].Skill.IsOptional));

                    if (SkillTarget != null)
                    {
                        yield return skills[i].Skill.Preview(SkillTarget);
                        if (player.IsAI)
                            WantComplete = true;
                    }
                    else if (WantSkillSkip)
                    {
                        skills[i].Skill.Skip();
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
        for (int j = skills.Length - 1; j >= 0; j--)
        {
            var prog = skills[j].Skill.PlayProgress;
            skills[j].Skill.Reset();
            if (prog == Skill.Progress.Queued ||
                prog == Skill.Progress.Skipped)
            {
                return j - 1;
            }
        }
        return -1;
    }
}