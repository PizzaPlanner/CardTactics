
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    public enum CardState
    {
        Unplayed,
        Current,
        Played,
        Skipped,
        Active,
        Destroyed
    }

    [ShowInInspector]
    [ReadOnly]
    public CardState State { get; protected set; }

    public virtual IEnumerator Play(Map map, PlayerInfo player, bool autocompleteIfPossible = true, Action onComplete = null)
    {
        State = CardState.Current;
        yield return new WaitForSeconds(0.2f);
        yield return PlaySkills(map, player, autocompleteIfPossible);
        State = StateOnPlayed;
        yield return new WaitForSeconds(0.2f);
        onComplete?.Invoke();
    }

    public void Destroy() { State = CardState.Destroyed; }

    public virtual void Reshuffle()
    {
        State = CardState.Unplayed;
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

    public void OnRoundStart(Map map, PlayerInfo player)
    {
        foreach (var skill in Skills)
        {
            skill.Prepare();
            if (State == CardState.Active && skill is Passive passive)
                passive.OnRoundStart(map, player);
        }
    }

    protected abstract CardState StateOnPlayed { get; }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public abstract int Speed { get; }

    [Required]
    public CardClass Class;
    public Card Source { get; private set; }

    protected abstract void OnInit();
    public void Init(Card source = null, CardState initialState = CardState.Unplayed)
    {
        State = initialState;
        Source = source;
        for (int i = 0; i < Skills.Length; i++)
        {
            Skills[i] = Skill.Instantiate(Skills[i]);
            Skills[i].Source = this;
        }
        OnInit();
    }

    public Skill[] Skills;
    public int[] Dependencies;
    public int[] Conditions;

    public void ExtendWith(Skill newSkillPrefab, Card source, int index, int dependency, int condition)
    {
        var newSkill = Instantiate(newSkillPrefab);
        newSkill.Source = source;
        var listS = Skills.ToList();
        listS.Insert(index, newSkill);
        Skills = listS.ToArray();

        var listD = Dependencies.ToList();
        listD.Insert(index, dependency);
        Dependencies = listD.ToArray();

        var listC = Conditions.ToList();
        listC.Insert(index, condition);
        Conditions = listC.ToArray();
    }

    public Tile SkillTarget;
    public bool WantSkillSkip;
    public bool WantComplete;
    public bool WantReset;

    protected IEnumerator PlaySkills(Map map, PlayerInfo player, bool autocompleteIfPossible)
    {
        if (Skills.Length == 0)
            WantComplete = true;
        for (int i = 0; i <= Skills.Length; i++)
        {
            if (i < Skills.Length)
            {
                SkillTarget = null;
                WantSkillSkip = false;
                WantComplete = autocompleteIfPossible;
                WantReset = false;
                Skill dependency = Dependencies[i] > 0 ? Skills[i - Dependencies[i]] : null;
                Skill condition = Conditions[i] > 0 ? Skills[i - Conditions[i]] : null;
                if (Skills[i].Activate(map, player, this, dependency, condition))
                {
                    if (!player.IsAI && SkillTarget == null)
                        map.ShowTileIndicators();

                    yield return new WaitUntil(() => SkillTarget != null || WantReset || (WantSkillSkip && Skills[i].IsOptional));

                    if (SkillTarget != null)
                    {
                        yield return Skills[i].Preview(SkillTarget);
                        if (player.IsAI)
                            WantComplete = true;
                    }
                    else if (WantSkillSkip)
                    {
                        Skills[i].Skip();
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
        for (int j = Skills.Length - 1; j >= 0; j--)
        {
            var prog = Skills[j].PlayProgress;
            Skills[j].Reset();
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