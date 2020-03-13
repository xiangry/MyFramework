using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurAppend : MonoBehaviour
{
    // Start is called before the first frame update

    private int bg = Shader.PropertyToID("_BG");
    private CameraRender ui_camera;
    public Material target;
    
    private Texture2D  ta;
    void Start()
    {
       
        gameObject.GetComponent<Image>().enabled = false;
        ui_camera = GameObject.FindGameObjectWithTag("GuiCamera").GetComponent<CameraRender>();
        ui_camera.enabled = true;
        RenderTexture rt = ui_camera.rt;
        RenderTexture.active = rt;
        
        ta = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32 , false);
        ta.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        ta.Apply();
        target.SetTexture(bg,ta);

        StartCoroutine(SetRender());

    }

    IEnumerator SetRender()
    {
        yield return new WaitForSeconds(0.1f);
        ui_camera.enabled = false;
        gameObject.GetComponent<Image>().enabled = true;
      
    }


    private void OnDestroy()
    {
        ta = null;
    }
}
