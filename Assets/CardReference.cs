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
    public Text ClassName;
    public Image ClassIcon;
    [SerializeField] bool ShowTooltipOnHover;

    public void UpdateVisuals()
    {
        if (Reference == null)
            return;
        Icon.sprite = Reference.Icon;
        if (Name != null)
            Name.text = Reference.Name;
        if (Cost != null)
            Cost.text = Reference.Cost.ToString();
        if (Speed != null)
            Speed.text = Reference.Speed.ToString();
        if (ClassName != null)
            ClassName.text = Reference.Class.Name;
        if (ClassIcon != null)
            ClassIcon.sprite = Reference.Class.Icon;

        foreach (var filter in GetComponentsInChildren<UICategoryFilter>(true))
            filter.gameObject.SetActive(filter.RequiredCategory == Reference.Class);
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