using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] Image Icon;
    [SerializeField] Text Text;

    public IEnumerator Set(Unit unit, string text)
    {
        Icon.sprite = unit.Icon;
        Text.text = text;
        gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }
}