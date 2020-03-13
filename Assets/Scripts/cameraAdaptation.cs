using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraAdaptation : MonoBehaviour
{
    public float initOrthoSize;

    public float initWidth;

    public float initHeight;

    private float factWidth;

    private float factHeight;
    // Start is called before the first frame update
    void Start()
    {
        factWidth = Screen.width;
        factHeight = Screen.height;
        //实际视口=初始视口*初始宽高比/实际宽高比
        GetComponent<Camera>().orthographicSize = (initOrthoSize * (initWidth / initHeight))/(factWidth/factHeight);
    }
    
}
