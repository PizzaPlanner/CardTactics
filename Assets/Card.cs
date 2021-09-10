
using UnityEngine;
using UnityEngine.UI;

public abstract class Card : ScriptableObject
{
    public abstract Texture2D Icon { get; }
    public abstract string Name { get; }
    public abstract int Speed { get; }
    public Skill[] Skills;

    [SerializeField] protected RectTransform fullCardPrefab;
    [SerializeField] protected RectTransform turnIndicatorPrefab;

    public RectTransform FullCard()
    {
        var item = GameObject.Instantiate(fullCardPrefab);
        item.GetComponent<CardReference>().Reference = this;
        item.GetComponent<CardReference>().Icon.sprite = Sprite.Create(Icon, new Rect(0, 0, Icon.width, Icon.height), Vector2.zero);
        item.GetComponent<CardReference>().Text.text = Name;
        return item;
    }

    public RectTransform TurnIndicator(Color playerColor)
    {
        var item = GameObject.Instantiate(turnIndicatorPrefab);
        item.GetComponent<CardReference>().Icon.sprite = Sprite.Create(Icon, new Rect(0, 0, Icon.width, Icon.height), Vector2.zero);
        item.GetComponent<CardReference>().Text.text = Name;
        item.GetChild(1).GetComponent<Image>().color = playerColor;
        return item;
    }
}