
using UnityEngine;
using UnityEngine.UI;

public abstract class Card : ScriptableObject
{
    public abstract Texture2D Icon { get; }
    public abstract string Name { get; }
    public abstract int Speed { get; }
    public abstract int Cost { get; }
    public abstract bool Once { get; }

    public Skill[] Skills;
    public int[] Dependencies;

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
}