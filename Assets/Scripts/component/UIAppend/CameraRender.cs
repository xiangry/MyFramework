using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRender : MonoBehaviour
{
    public RenderTexture rt;
 
    //src和des是引擎创建的对象，我们只能使用，但是不要自己引用到本地。
    private void OnRenderImage(RenderTexture src, RenderTexture des)
    {
        Graphics.Blit(src, des);//这一句一定要有，保证画面传递下去。不然会中断camera画面，别的滤镜会失效。
 
        //这里要拷贝渲染出一张rt
        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(src.width, src.height);//unity自己在维护一个RenderTexture池，我们从缓存中取就行。
        }
        Graphics.Blit(src,rt);//拷贝图像
    }
 
    private void OnDestroy()
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);//记得释放引用
        }
    }
}
