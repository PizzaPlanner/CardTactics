using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] Image Icon;
    [SerializeField] Text Text;

    Action OnClose;
    public void Set(Unit unit, string text, Action onClose)
    {
        Icon.sprite = unit.Icon;
        Text.text = text;
        OnClose = onClose;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
        }
    }
}