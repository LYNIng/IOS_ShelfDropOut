using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBGScale : MonoBehaviour
{

    void Start()
    {
        var p = (float)Screen.width / Screen.height;

        if (p > 0.6f)
        {
            transform.localScale = Vector3.one * 0.8f;
        }
        else
        {
            transform.localScale = Vector3.one * 0.97f;
        }
    }

}
