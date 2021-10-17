using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class UnitDisplay : MonoBehaviour
{
    [SerializeField] Text Life, MaxLife;

    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as UnitCard;
        if (Reference == null) return;

        if (Life != null)
            Life.text = Reference.UnitReference.LifePreview.ToString();
        if (MaxLife != null)
            MaxLife.text = Reference.UnitReference.MaxLife.ToString();
    }
}
