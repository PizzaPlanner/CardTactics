using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class UnitDisplay : MonoBehaviour
{
    public UnitCard Reference;

    [SerializeField] Text Life, MaxLife, Passive;
    [SerializeField] RectTransform CapacityPanel;

    [SerializeField] Image CapacityIconPrefab;

    void OnEnable()
    {
        if (Reference == null) return;

        if (Life != null)
            Life.text = Reference.UnitReference.LifePreview.ToString();
        if (MaxLife != null)
            MaxLife.text = Reference.UnitReference.MaxLife.ToString();
        if (Passive != null)
            Passive.text = Reference.Passive != null ? Reference.Passive.Passive.Description : "";
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
