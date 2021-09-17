using UnityEngine;
using UnityEngine.UI;

public class UnitStatusBar : MonoBehaviour
{
    RectTransform bar;

    Text Life;

    [SerializeField] RectTransform barPrefab;

    void Start()
    {
        bar = Instantiate(barPrefab);
        bar.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform, false);
        bar.transform.SetAsFirstSibling();
        Life = bar.gameObject.GetComponentInChildren<Text>();
    }

    void Update()
    {
        bar.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        Life.text = gameObject.GetComponentInParent<Unit>().LifePreview.ToString();
    }

    void OnDestroy()
    {
        if (bar != null && bar.gameObject != null)
            GameObject.Destroy(bar.gameObject);
    }
}
