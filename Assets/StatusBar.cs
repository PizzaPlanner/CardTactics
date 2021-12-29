using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    Unit Unit;
    Material Mat;

    public bool Focus, Current;

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
        if ((Focus || Current) && highlight < 1)
            Mat.SetFloat("_highlight", highlight + Time.deltaTime * 10);
        else if (!(Focus || Current) && highlight > 0)
            Mat.SetFloat("_highlight", highlight - Time.deltaTime * 10);

        var current = Mat.GetFloat("_current");
        if (Current && current < 1)
            Mat.SetFloat("_current", current + Time.deltaTime * 10);
        else if (!Current && current > 0)
            Mat.SetFloat("_current", current - Time.deltaTime * 10);
    }
}
