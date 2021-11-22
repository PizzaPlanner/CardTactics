
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
        Debug.Log("[Card] Play " + Name);
        State = CardState.Current;
        CardTooltip.Show(this);
        OnPlay(board, map, unit, player);
        yield return new WaitForSeconds(board.CardPlayPreTime);
        yield return PlaySkills(map, unit, player, true);
        yield return new WaitForSeconds(board.CardPlayPostTime);
        CardTooltip.Hide();
        State = StateOnPlayed;
        foreach (var passive in passives)
            passive.OnCardRemoved(unit);
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

    public virtual void OnRoundStart(Map map, Unit unit)
    {
        foreach (var skill in Skills)
            skill.Prepare();
    }

    public virtual void OnTurnStart(Map map, Unit unit)
    {
        if (State == CardState.Unplayed)
            foreach (var passive in passives)
                passive.OnTurnStart(unit);
    }

    public virtual void OnTurnEnd(Map map, Unit unit)
    {
        if (State == CardState.Unplayed)
            foreach (var passive in passives)
                passive.OnTurnEnd(unit);
    }

    public virtual void OnRoundEnd(Map map, Unit unit)
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

        if (skills == null)
            skills = new SkillInfo[0];
        if (passives == null)
            passives = new Passive[0];

        foreach (var skill in skills)
            skill.Skill.Source = this;
        foreach (var passive in passives)
            passive.OnCardCreated(this);

        OnInit();
    }

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    SkillInfo[] skills;

    [SerializeReference]
    [TableList(AlwaysExpanded = true)]
    Passive[] passives;


    public IEnumerable<Skill> Skills => skills?.Select(x => x.Skill) ?? new Skill[0];

    public void ExtendWith(SkillInfo newSkill, int index)
    {
        var listS = skills.ToList();
        listS.Insert(index, newSkill);
        skills = listS.ToArray();
    }

    public virtual string GetDescription()
    {
        var descriptions = new List<string>();
        if (passives != null && passives.Length > 1)
        {
            descriptions.Add("While in your Hand:");
            foreach (var skill in passives)
                if (skill.Description != null)
                    descriptions.Add(skill.Description);
            descriptions.Add("--------");
        }
        foreach (var skill in Skills)
            if (skill.Description != null)
                descriptions.Add(skill.Description);
        return string.Join("\n", descriptions);
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
                    Debug.Log($"[Card] Use Skill {skills[i].Skill.GetType().Name}: <color=green>OK</color>");
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
                else
                    Debug.Log($"[Card] Use Skill {skills[i].Skill.GetType().Name}: <color=red>Condition Failed</color>");
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