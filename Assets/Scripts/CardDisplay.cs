using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeReference] public Card Reference;

    public Image Icon;
    public Text Name;
    public Text Speed;
    public Text ClassName;
    public Text SkillPanel;
    public Image ClassIcon;
    public Image PlayerColorBorder;
    [SerializeField] bool ShowTooltipOnHover;

    public void UpdateVisuals()
    {
        if (Reference == null)
            return;
        Icon.sprite = Reference.Icon;
        if (Name != null)
            Name.text = Reference.Name;
        if (Speed != null)
            Speed.text = Reference.Speed.ToString();
        if (ClassName != null)
            ClassName.text = Reference.Class.Name;
        if (ClassIcon != null)
            ClassIcon.sprite = Reference.Class.Icon;
        if (SkillPanel != null)
        {
            var descriptions = new List<string>();
            for (int i = 0; i < Reference.Skills.Length; i++)
            {
                if (Reference.Conditions[i] == 0 && descriptions.Count > 0)
                {
                    descriptions.Add("-----");
                }
                if (Reference.Skills[i].Description != null)
                    descriptions.Add(Reference.Skills[i].Description);
            }
            SkillPanel.text = string.Join("\n", descriptions);
        }
    }

    public void SetCard(Card card)
    {
        Reference = card;
        foreach (var filter in GetComponentsInChildren<UICategoryFilter>(true))
            filter.gameObject.SetActive(filter.RequiredCategory == Reference.Class);

        if (GetComponent<UnitDisplay>() != null && card is UnitCard)
            GetComponent<UnitDisplay>().UpdateVisuals();
        if (GetComponent<ActionDisplay>() != null && card is ActionCard)
            GetComponent<ActionDisplay>().UpdateVisuals();
        UpdateVisuals();


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShowTooltipOnHover && Reference != null)
        {
            CardTooltip.Show(Reference);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CardTooltip.Hide();
    }
}