using UnityEngine;
using UnityEngine.UI;

public class UnitStatusBar : MonoBehaviour
{
    RectTransform bar;

    Text Life;
    Image fillBar;

    [SerializeField] RectTransform barPrefab;

    Unit unit;

    float speed = 0;

    void Start()
    {
        bar = Instantiate(barPrefab);
        bar.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform, false);
        bar.transform.SetAsFirstSibling();
        Life = bar.GetChild(0).GetChild(0).GetComponentInChildren<Text>();
        fillBar = bar.GetChild(0).GetChild(0).GetComponentInChildren<Image>();
        unit = gameObject.GetComponentInParent<Unit>();
        fillBar.color = unit.Player.Team.Color;
    }

    void Update()
    {
        bar.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        Life.text = gameObject.GetComponentInParent<Unit>()?.LifePreview.ToString();
        var diff = fillBar.fillAmount - unit.LifePreview / unit.MaxLife;
        if (Mathf.Abs(diff) > 0.001f)
        {
            if (speed == 0)
                speed = diff;
            fillBar.fillAmount -= Time.deltaTime * speed;
            fillBar.fillAmount = Mathf.Clamp01(fillBar.fillAmount);
        }
        else
        {
            speed = 0;
        }
    }

    void OnDestroy()
    {
        if (bar != null && bar.gameObject != null)
            GameObject.Destroy(bar.gameObject);
    }
}
