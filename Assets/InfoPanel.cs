using System;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    static InfoPanel panel;

    [SerializeField] Text text;
    [SerializeField] Image image;

    void Start()
    {
        panel = this;
        Hide();
    }

    public static void Show(Tile tile)
    {
        if (tile.Unit != null)
        {
            panel.text.text = $"{tile.Unit.Name}  (steht auf {tile.name})";
            panel.image.sprite = Sprite.Create(tile.Unit.Icon, new Rect(0, 0, tile.Unit.Icon.width, tile.Unit.Icon.height), Vector2.zero);
            panel.image.gameObject.SetActive(true);
        }
        else
        {
            panel.text.text = $"{tile.name} (leer)";
            panel.image.gameObject.SetActive(false);
        }
        panel.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        panel.gameObject.SetActive(false);
    }
}