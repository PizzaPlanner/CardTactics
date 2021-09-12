using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    public Skill Skill;
    public bool FromOtherSource;

    [SerializeField] Image Icon;
    [SerializeField] Text Name;

    // Start is called before the first frame update
    void Start()
    {
        Icon.sprite = Skill.Icon;
        Name.text = Skill.Name;
        if (FromOtherSource)
            Name.text += " <color=green>(Bonus)</color>";
    }

}
