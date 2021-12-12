
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public struct PlayableMessage : NetworkMessage
{
    public IPlayableContext context;
}

public static class ContextSerializationHelper
{

    public static void WriteSkill(this NetworkWriter writer, ISkill skill)
    {
        writer.WriteString(skill.GetType().Name);
        writer.WriteString(skill.GetType().Assembly.GetName().Name);
        skill.Serialize(writer);
    }

    public static ISkill ReadSkill(this NetworkReader reader)
    {
        var type = reader.ReadString();
        var asm = reader.ReadString();
        var inst = (ISkill)System.Activator.CreateInstance(asm, type).Unwrap();
        return inst.Deserialize(reader);
    }

    public static void WriteSkillInfo(this NetworkWriter writer, SkillInfo info)
    {

        writer.Write((byte)info.SourceTileInput);
        writer.Write((byte)info.TileOutput);
        if (info.Conditions != null)
        {
            writer.Write(info.Conditions.Length);
            foreach (var cond in info.Conditions)
                writer.Write((byte)cond);
        }
        else writer.WriteInt(0);
        writer.WriteSkill(info.Skill);
    }

    public static SkillInfo ReadSkillInfo(this NetworkReader reader)
    {
        var info = new SkillInfo
        {
            SourceTileInput = (SkillChain)reader.ReadByte(),
            TileOutput = (SkillChain)reader.ReadByte()
        };
        var arr = new SkillChain[reader.ReadInt()];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = (SkillChain)reader.ReadByte();
        info.Conditions = arr;
        info.Skill = reader.ReadSkill();
        return info;
    }

    public static void WritePlayable(this NetworkWriter writer, IPlayableContext context)
    {
        writer.WriteString(context.GetType().Name);
        writer.WriteString(context.GetType().Assembly.GetName().Name);
        context.Serialize(writer);
    }

    public static IPlayableContext ReadPlayable(this NetworkReader reader)
    {
        var type = reader.ReadString();
        var asm = reader.ReadString();
        var inst = (IPlayableContext)System.Activator.CreateInstance(asm, type).Unwrap();
        return inst.Deserialize(reader);
    }
}

public interface IPlayableContextWithUnit : IPlayableContext
{
    Unit Unit { get; }
}

public interface IPlayableContext
{
    IEnumerator PlayOnServer();
    IEnumerator PlayOnClient();
    bool Queueable { get; }
    string Name { get; }

    void Serialize(NetworkWriter writer);
    IPlayableContext Deserialize(NetworkReader reader);
}

public class CardContext : IPlayableContextWithUnit
{
    [Sirenix.OdinInspector.ShowInInspector]
    public Card Target { get; private set; }
    [Sirenix.OdinInspector.ShowInInspector]
    public CardState State;

    public Map Map { get; private set; }

    public Unit Unit { get; private set; }

    public List<SkillContext> PreviousContexts { get; private set; }

    public bool Queueable => State == CardState.Unplayed;
    public string Name => Target.Name;

    public CardContext(Card card, Map map, Unit unit, CardState initialState)
    {
        Target = card;
        Map = map;
        Unit = unit;
        State = initialState;
        PreviousContexts = new List<SkillContext>();
    }

    public IEnumerator PlayOnClient()
    {
        CardTooltip.Show(Target);
        yield return GameSettings.Instance.BeforePlayDelay.Yield();
    }

    public IEnumerator PlayOnServer()
    {
        if (Unit == null || Unit.IsDead) yield break;

        State = CardState.Current;
        PreviousContexts = new List<SkillContext>();
        Target.OnPlay(this);
        for (int i = 0; i < Target.Skills.Count(); i++)
        {
            var skill = Target.skills.ToArray()[i];
            var ctx = new SkillContext(skill, Map, Map.TileOf(Unit), PreviousContexts.ToArray());
            Board.FindObjectOfType<Board>().Playables.Enqueue(ctx);
            PreviousContexts.Add(ctx);
        }
        Board.FindObjectOfType<Board>().Playables.Enqueue(new FinishCardContext(this));
        yield return null;
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.WriteCard(Target);
        writer.Write((byte)State);
        writer.WriteNetworkBehaviour(Map);
        writer.WriteNetworkBehaviour(Unit);
    }

    public IPlayableContext Deserialize(NetworkReader reader)
    {
        Target = reader.ReadCard();
        State = (CardState)reader.ReadByte();
        Map = reader.ReadNetworkBehaviour<Map>();
        Unit = reader.ReadNetworkBehaviour<Unit>();
        return this;
    }

    public class FinishCardContext : IPlayableContext
    {
        public FinishCardContext(CardContext origin)
        {
            context = origin;
        }

        public bool Queueable => true;

        public string Name => "Finish " + context.Name;

        public Unit Unit => context.Unit;

        CardContext context;

        public IEnumerator PlayOnClient()
        {
            CardTooltip.Hide();
            yield return GameSettings.Instance.AfterPlayDelay.Yield();
            context.PreviousContexts.Clear();
        }

        public IEnumerator PlayOnServer()
        {
            context.State = context.Target.StateOnPlayed;
            foreach (var passive in context.Target.passives)
                passive.OnCardRemoved(context);
            context.PreviousContexts.Clear();
            yield return null;
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

    public int Id => id;
    [SerializeField] int id;

    [TextArea]
    [SerializeField] string DescriptionOverride;

    void OnEnable()
    {
        if (skills == null)
            skills = new SkillInfo[0];
        if (passives == null)
            passives = new Passive[0];

        foreach (var passive in passives)
            passive.Source = this;
    }

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    public SkillInfo[] skills;

    [SerializeReference]
    [TableList(AlwaysExpanded = true)]
    public Passive[] passives;


    public IEnumerable<ISkill> Skills => skills?.Select(x => x.Skill) ?? new ISkill[0];

    protected virtual string GetDescription()
    {
        if (!string.IsNullOrEmpty(DescriptionOverride))
            return DescriptionOverride;

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
            var contents = substr[2..^1];
            var type = substr[1];
            startIdx = start;
            try
            {
                var split = contents.Split(':');
                object target = type switch
                {
                    '!' => skills[int.Parse(split[0])].Skill,
                    '+' => passives[int.Parse(split[0])],
                    _ => null,
                };
                if (target != null)
                {
                    FieldInfo field = target.GetType().GetField(split[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                        replacement = $"<color=#2288ff>{field.GetValue(target)}</color>";
                    else
                        replacement = $"<color=#ff3333>No Data for '{split[1]}'</color>";
                }
            }
            catch (Exception e) { replacement = $"<color=#ff3333>Invalid Format</color>"; }
            desc = desc.Replace(substr, replacement);
        }
        return desc;
    }

    public override string ToString() => Name;
}