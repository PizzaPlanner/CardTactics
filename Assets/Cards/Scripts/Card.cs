
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public interface IPlayableContext
{
    IEnumerator Play();
    bool Queueable { get; }
    string Name { get; }
    PlayerInfo Player { get; }
}

public class CardContext : IPlayableContext
{
    [ShowInInspector]
    public Card Target { get; private set; }
    [ShowInInspector]
    public CardState State;

    public Map Map { get; private set; }

    public Unit Unit { get; private set; }

    public int currentSkill { get; private set; }

    public List<SkillContext> PreviousContexts { get; private set; }

    public bool Queueable => State == CardState.Unplayed;
    public string Name => Target.Name;
    public PlayerInfo Player => Unit.Player;

    public CardContext(Card card, Map map, Unit unit, CardState initialState)
    {
        Target = card;
        Map = map;
        Unit = unit;
        State = initialState;
        PreviousContexts = new List<SkillContext>();
    }

    public IEnumerator Play()
    {
        State = CardState.Current;
        CardTooltip.Show(Target);
        currentSkill = 0;
        PreviousContexts = new List<SkillContext>();
        Target.OnPlay(this);
        // yield return new WaitForSeconds(board.CardPlayPreTime);
        for (currentSkill = 0; currentSkill < Target.Skills.Count(); currentSkill++)
        {
            var skill = Target.skills.ToArray()[currentSkill];
            var ctx = new SkillContext(skill, this, Player, Map.TileOf(Unit));
            yield return ctx.Play();
            PreviousContexts.Add(ctx);
        }
        // yield return new WaitForSeconds(board.CardPlayPostTime);
        CardTooltip.Hide();
        PreviousContexts.Clear();
        State = Target.StateOnPlayed;
        foreach (var passive in Target.passives)
            passive.OnCardRemoved(this);
    }
}
public enum CardState
{
    Unplayed,
    Played,
    Current,
    Active,
    Destroyed
}

public abstract class Card : ScriptableObject, IPlayable
{
    public virtual void OnPlay(CardContext context) { }

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

    public virtual void OnRoundStart(CardContext context)
    {
    }

    public virtual void OnTurnStart(CardContext context)
    {
        foreach (var passive in passives)
            passive.OnTurnStart(context);
    }

    public virtual void OnTurnEnd(CardContext context)
    {
        foreach (var passive in passives)
            passive.OnTurnEnd(context);
    }

    public virtual void OnRoundEnd(CardContext context)
    {
        if (context.State == CardState.Played)
            context.State = CardState.Unplayed;
    }

    public abstract CardState StateOnPlayed { get; }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public Card Source { get; private set; }

    void OnEnable()
    {
        if (skills == null)
            skills = new SkillInfo[0];
        if (passives == null)
            passives = new Passive[0];

        foreach (var skill in skills)
            skill.Skill.Source = this;
        foreach (var passive in passives)
            passive.Source = this;
    }

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    public SkillInfo[] skills;

    [SerializeReference]
    [TableList(AlwaysExpanded = true)]
    public Passive[] passives;


    public IEnumerable<Skill> Skills => skills?.Select(x => x.Skill) ?? new Skill[0];

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
}