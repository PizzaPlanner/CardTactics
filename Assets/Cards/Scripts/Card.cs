
using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    public enum CardState
    {
        Unplayed,
        Played,
        Skipped,
        Active,
        Destroyed
    }

    [ShowInInspector]
    [ReadOnly]
    public CardState State { get; protected set; }

    public virtual IEnumerator Play(GameSettings settings, GameSettings.PlayerInfo player, Action onComplete)
    {
        yield return PlaySkills(settings, player);
        State = StateOnPlayed;
        onComplete?.Invoke();
    }

    public void Destroy() { State = CardState.Destroyed; }

    public virtual void Skip(GameSettings settings, GameSettings.PlayerInfo player)
    {
        for (int i = Skills.Length - 1; i >= 0; i--)
        {
            if (Skills[i].PlayProgress == Skill.Progress.Queued)
                Skills[i].Reset();
        }

        State = StateOnSkipped;
    }

    public virtual void Reshuffle()
    {
        State = CardState.Unplayed;
    }

    public bool IsVirtual;

    protected abstract CardState StateOnPlayed { get; }
    protected abstract CardState StateOnSkipped { get; }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public abstract int Speed { get; }
    public abstract int Cost { get; }
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
    public bool WantCardSkip;
    public bool WantComplete;
    public bool WantReset;

    protected IEnumerator PlaySkills(GameSettings settings, GameSettings.PlayerInfo player)
    {
        WantCardSkip = false;
        for (int i = 0; i <= Skills.Length; i++)
        {
            if (i < Skills.Length)
            {
                SkillTarget = null;
                WantSkillSkip = false;
                WantComplete = false;
                WantReset = false;
                Skill dependency = Dependencies[i] > 0 ? Skills[i - Dependencies[i]] : null;
                Skill condition = Conditions[i] > 0 ? Skills[i - Conditions[i]] : null;
                if (Skills[i].Activate(player, this, dependency, condition))
                {
                    yield return new WaitUntil(() => SkillTarget != null || WantCardSkip || WantReset || (WantSkillSkip && Skills[i].IsOptional));

                    if (SkillTarget != null)
                    {
                        Skills[i].Preview(SkillTarget);
                    }
                    else if (WantSkillSkip)
                    {
                        Skills[i].Skip();
                    }
                    else if (WantReset)
                    {
                        i = PartialReset();
                    }
                    if (WantCardSkip)
                    {
                        i = Skills.Length - 1;
                    }
                }
                settings.Map.HideTileSelectionPreview();
            }
            else
            {
                yield return new WaitUntil(() => WantCardSkip || WantComplete || WantReset);
                if (WantCardSkip)
                {
                    Skip(settings, player);
                }
                else if (WantComplete)
                {
                    foreach (var skill in Skills)
                    {
                        if (skill.PlayProgress == Skill.Progress.Queued)
                            skill.OnUse();
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
                Debug.Log("i is now " + (j - 1));
                return j - 1;
            }
        }
        return -1;
    }
}