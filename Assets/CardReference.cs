using UnityEngine;
using UnityEngine.UI;

public class CardReference : MonoBehaviour
{
    [SerializeReference] public Card Reference;

    public Image Icon;
    public Text Name;
    public Text Cost;
    public Text Speed;

    void Start()
    {
        Icon.sprite = Sprite.Create(Reference.Icon, new Rect(0, 0, Reference.Icon.width, Reference.Icon.height), Vector2.zero);
        if (Name != null)
            Name.text = Reference.Name;
        if (Cost != null)
            Cost.text = Reference.Cost.ToString();
        if (Speed != null)
            Speed.text = Reference.Speed.ToString();
    }
}