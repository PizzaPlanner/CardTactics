using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardReference : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeReference] public Card Reference;

    public Image Icon;
    public Text Name;
    public Text Cost;
    public Text Speed;
    public Text Category;
    [SerializeField] bool ShowTooltipOnHover;

    public void UpdateVisuals()
    {
        if (Reference == null)
            return;
        Icon.sprite = Sprite.Create(Reference.Icon, new Rect(0, 0, Reference.Icon.width, Reference.Icon.height), Vector2.zero);
        if (Name != null)
            Name.text = Reference.Name;
        if (Cost != null)
            Cost.text = Reference.Cost.ToString();
        if (Speed != null)
            Speed.text = Reference.Speed.ToString();
        if (Category != null)
            Category.text = Reference.Category;
    }

    void Start()
    {
        UpdateVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShowTooltipOnHover && Reference != null)
            SkillDisplay.Show(Reference);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillDisplay.Hide();
    }
}