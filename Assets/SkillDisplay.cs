using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour
{
    [SerializeField] RectTransform panel;
    [SerializeField] SkillItem prefab;

    public Card Reference { get; private set; }

    static SkillDisplay instance;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(Card card)
    {
        instance.Reference = card;
        instance.GetComponent<CardReference>().Reference = card;
        instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (Reference == null)
        {
            Hide();
            return;
        }
        foreach (var leftover in panel.GetComponentsInChildren<SkillItem>())
            Destroy(leftover.gameObject);
        foreach (var skill in Reference.Skills)
        {
            var item = GameObject.Instantiate(prefab);
            item.Skill = skill;
            item.transform.SetParent(panel);
        }
    }
}
