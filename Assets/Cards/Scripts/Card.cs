
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

    public CardState State { get => state; protected set { OnStateChange(state, value); state = value; } }
    CardState state;

    public virtual void Play(GameSettings settings, GameSettings.PlayerInfo player) { State = CardState.Played; }

    public void Destroy() { State = CardState.Destroyed; }

    public virtual void Skip(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Skipped;
    }

    public virtual void Reshuffle()
    {
        State = CardState.Unplayed;
    }

    protected virtual void OnStateChange(CardState oldState, CardState newState) { }

    public bool IsVirtual;

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
}