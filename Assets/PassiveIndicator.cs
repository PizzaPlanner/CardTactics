using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassiveIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Passive Passive;

    void Start()
    {
        GetComponentInChildren<Image>().sprite = Passive.Icon;
        GetComponentInChildren<Text>().text = Passive.Name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Passive.Source != null)
            SkillDisplay.Show(Passive.Source);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillDisplay.Hide();
    }
}
