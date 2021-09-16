using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

[RequireComponent(typeof(CardDisplay))]
public class PassiveDisplay : MonoBehaviour
{
    [SerializeField] Text Text;
    public void UpdateVisuals()
    {
        var Reference = GetComponent<CardDisplay>().Reference as PassiveCard;
        if (Reference == null) return;

        if (Text != null)
            Text.text = Reference.Passive.Description;
    }
}
