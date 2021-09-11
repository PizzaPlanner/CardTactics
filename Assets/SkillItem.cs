using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    public Skill Skill;

    [SerializeField] Image Icon;
    [SerializeField] Text Name;

    // Start is called before the first frame update
    void Start()
    {
        Icon.sprite = Skill.Icon;
        Name.text = Skill.Name;
    }

}
