using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class UnitDisplay : MonoBehaviour
{
    [SerializeField] Text Life, MaxLife, PassiveText;
    [SerializeField] Image PassiveIcon;
    [SerializeField] RectTransform CapacityPanel;

    [SerializeField] Image CapacityIconPrefab;

    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as UnitCard;
        if (Reference == null) return;

        if (Life != null)
            Life.text = Reference.UnitReference.LifePreview.ToString();
        if (MaxLife != null)
            MaxLife.text = Reference.UnitReference.MaxLife.ToString();
        if (PassiveText != null)
            PassiveText.text = Reference.Passive != null ? Reference.Passive.Name : "";
        if (PassiveIcon != null)
            PassiveIcon.sprite = Reference.Passive?.Icon;
        if (CapacityPanel != null)
        {
            foreach (Transform t in CapacityPanel)
                Destroy(t.gameObject);
            foreach (var capacity in Reference.Capacities)
            {
                var itm = Instantiate(CapacityIconPrefab);
                itm.sprite = capacity.Class.Icon;
                itm.GetComponentInChildren<Text>().text = capacity.MaxNumber.ToString();
                itm.transform.SetParent(CapacityPanel);
            }
        }

    }
}
