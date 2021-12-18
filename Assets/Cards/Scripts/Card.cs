
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;


public class CardContext : IPlayableContextWithUnit, IClientPlayable, IServerPlayable
{
    [Sirenix.OdinInspector.ShowInInspector]
    public Card Target { get; private set; }
    [Sirenix.OdinInspector.ShowInInspector]
    public CardState State;

    public Unit Unit { get; private set; }
    public NetworkConnection Owner => Unit.Owner.connectionToClient;

    public bool Queueable => State == CardState.Unplayed || State == CardState.TurnStart || State == CardState.TurnEnd;
    public string Name => Target.Name;
    public bool ManualSending => true;

    public CardContext(Card card, Unit unit, CardState initialState)
    {
        Target = card;
        Unit = unit;
        State = initialState;
    }
    public CardContext() { }

    public IEnumerator PlayOnClient()
    {
        CardTooltip.Show(Target);
        yield return GameSettings.Instance.BeforePlayDelay.Yield();
    }

    public IEnumerator PlayOnServer()
    {
        if (Unit == null || Unit.IsDead) yield break;

        IEnumerable<SkillInfo> skillsToPlay = State switch
        {
            CardState.TurnStart => Target.skills.Where(x => x.Trigger == SkillTrigger.TurnStart),
            CardState.TurnEnd => Target.skills.Where(x => x.Trigger == SkillTrigger.TurnEnd),
            CardState.Unplayed => Target.skills.Where(x => x.Trigger == SkillTrigger.Use),
            _ => null,
        };

        if (State == CardState.Unplayed)
            Target.OnPlay(this);

        var newstate = State == Target.DestroyedOn ? CardState.Destroyed : State switch
        {
            CardState.Unplayed => CardState.Played,
            CardState.TurnStart => CardState.Unplayed,
            CardState.TurnEnd => CardState.Shuffle,
            _ => CardState.Destroyed
        };

        if (skillsToPlay == null || skillsToPlay.Count() == 0)
            yield break;

        NetworkServer.SendToAll(new PlayableMessage { context = this });

        var previousContexts = new List<SkillContext>();

        var map = Map.FindObjectOfType<Map>();
        var playables = RoomNetworkManager.Playables;
        for (int i = 0; i < skillsToPlay.Count(); i++)
        {
            var skill = skillsToPlay.ToArray()[i];
            var ctx = new SkillContext(skill, map, map.TileOf(Unit), previousContexts.ToArray());
            playables.Enqueue(ctx);
            previousContexts.Add(ctx);
        }
        State = newstate;
        playables.Enqueue(new FinishCardContext(this));
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteObject(Target);
        writer.Write((byte)State);
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Target = reader.ReadObject<Card>();
        State = (CardState)reader.ReadByte();
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }
}

public class FinishCardContext : IPlayableContext, IClientPlayable
{
    public FinishCardContext(CardContext origin)
    {
        context = origin;
    }
    public FinishCardContext() { }

    public bool Queueable => true;

    public string Name => "Finish " + context.Name;

    public Unit Unit => context.Unit;

    CardContext context;

    public IEnumerator PlayOnClient()
    {
        CardTooltip.Hide();
        yield return GameSettings.Instance.AfterPlayDelay.Yield();
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WritePlayable(context);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        return new FinishCardContext((CardContext)reader.ReadPlayable());
    }
}

public enum CardState
{
    Shuffle,
    TurnStart,
    Unplayed,
    Played,
    Destroyed,
    TurnEnd
}

public abstract class Card : ScriptableObject, IPlayable
{
    public virtual void OnPlay(CardContext context) { }
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public Card Source { get; private set; }

    public bool Playable => skills.Any(x => x.Trigger == SkillTrigger.Use);

    public abstract CardState DestroyedOn { get; }

    [BoxGroup("Override Description")]
    [HideLabel]
    [TextArea(2, 15)]
    [SerializeField] string DescriptionOverride;

    void OnEnable()
    {
        if (skills == null)
            skills = new SkillInfo[0];
    }

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    public SkillInfo[] skills;

    public IEnumerable<ISkill> Skills => skills?.Select(x => x.Skill) ?? new ISkill[0];

    protected virtual string GetDescription()
    {
        if (!string.IsNullOrEmpty(DescriptionOverride))
            return DescriptionOverride;

        var descriptions = new List<string>();
        foreach (var skill in Skills)
            if (skill.Description != null)
                descriptions.Add(skill.Description);
        return string.Join("\n", descriptions);
    }

    public string GetFormattedDescription()
    {
        var desc = GetDescription();
        var startIdx = -1;
        while (desc.IndexOf('[') > startIdx)
        {
            var replacement = "<color=#ff3333>Invalid Link</color>";
            var start = desc.IndexOf("[");
            var end = desc[start..].IndexOf("]") + start + 1;
            var substr = desc[start..end];
            var contents = substr[1..^1];
            startIdx = start;
            try
            {
                var split = contents.Split(':');
                var target = skills[int.Parse(split[0])].Skill;
                if (target != null)
                {
                    FieldInfo field = target.GetType().GetField(split[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                        replacement = $"<color=#88aaff>{field.GetValue(target)}</color>";
                    else
                        replacement = $"<color=#ff3333>No Data for '{split[1]}'</color>";
                }
            }
            catch (Exception e) { replacement = $"<color=#ff3333>Invalid Format</color>"; }
            desc = desc.Replace(substr, replacement);
        }
        desc = desc
        .Replace("!play", "<sprite=\"classicons\" name=\"event_play\">")
        .Replace("!start", "<sprite=\"classicons\" name=\"event_turnstart\">")
        .Replace("!end", "<sprite=\"classicons\" name=\"event_turnend\">");
        return desc;
    }

    public override string ToString() => Name;
}