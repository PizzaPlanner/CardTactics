using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeSeeThrough : MonoBehaviour
{
    void OnEnable()
    {
        foreach (var child in GetComponentsInChildren<Renderer>())
            child.gameObject.layer = LayerMask.NameToLayer("SeeThrough");
    }
}
