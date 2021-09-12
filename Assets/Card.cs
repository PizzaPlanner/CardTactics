
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

    public abstract void Play(GameSettings settings, GameSettings.PlayerInfo player);
    public virtual void Skip(GameSettings settings, GameSettings.PlayerInfo player)
    {
        State = CardState.Skipped;
    }

    public virtual void Reshuffle()
    {
        State = CardState.Unplayed;
    }

    protected virtual void OnStateChange(CardState oldState, CardState newState) { }

    public abstract Texture2D Icon { get; }
    public abstract string Name { get; }
    public abstract int Speed { get; }
    public abstract int Cost { get; }
    public abstract bool Once { get; }
    public abstract string Category { get; }

    protected abstract void OnInit();
    public void Init()
    {
        State = CardState.Unplayed;
        for (int i = 0; i < Skills.Length; i++)
        {
            Skills[i] = Skill.Instantiate(Skills[i]);
            Skills[i].Source = this;
        }
    }

    public Skill[] Skills;
    public int[] Dependencies;
    public int[] Conditions;

    [SerializeField] protected RectTransform fullCardPrefab;
    [SerializeField] protected RectTransform turnIndicatorPrefab;

    public RectTransform FullCard()
    {
        var item = GameObject.Instantiate(fullCardPrefab);
        item.GetComponent<CardReference>().Reference = this;
        return item;
    }

    public RectTransform TurnIndicator(Color playerColor)
    {
        var item = GameObject.Instantiate(turnIndicatorPrefab);
        item.GetComponent<CardReference>().Reference = this;
        item.GetChild(1).GetComponent<Image>().color = playerColor;
        return item;
    }

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