using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    Unit Unit;
    Material Mat;

    public bool Focus;

    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponentInParent<Unit>();
        Mat = GetComponent<MeshRenderer>().material;
        Mat.SetColor("_healthColor", Unit.Player.TeamColor);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Mat.SetFloat("_clip", Unit.DisplayLife / Unit.MaxLife);
        transform.rotation = Quaternion.identity;

        var highlight = Mat.GetFloat("_highlight");
        if (Focus && highlight < 1)
            Mat.SetFloat("_highlight", highlight + Time.deltaTime * 5);
        else if (!Focus && highlight > 0)
            Mat.SetFloat("_highlight", highlight - Time.deltaTime * 5);
    }
}
