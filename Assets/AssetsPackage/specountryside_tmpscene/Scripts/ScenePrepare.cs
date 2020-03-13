using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using Sword;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenePrepare : MonoBehaviour
{
#if UNITY_EDITOR
    private static string lastScene;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (SceneManager.GetActiveScene().name != lastScene)
        {
            lastScene = SceneManager.GetActiveScene().name;
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var go in gameObjects)
            {
                Image[] images = go.GetComponentsInChildren<Image>(true);
                for (int j = 0; j < images.Length; j++)
                {
                    int que = images[j].material.renderQueue;
                    images[j].material.shader = Shader.Find(images[j].material.shader.name);
                    if(images[j].material.renderQueue != que)
                    {
//                    Logger.LogError($"{images[j].name}重新设置shander后，渲染序列改变 {que} => {images[j].material.renderQueue}");
                        images[j].material.renderQueue = que;
                    }
                }

                Renderer[] meshSkinRenderer = go.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j< meshSkinRenderer.Length; j++)
                {   
                    if (meshSkinRenderer[j].sharedMaterial == null)
                    {
                        continue;
                    }
                
                    int que = 0;
                    Material[] materials = meshSkinRenderer[j].sharedMaterials;
                    foreach (var material in materials)
                    {
                        if (!material.IsNull())
                        {
                            que = material.renderQueue;
                            material.shader = Shader.Find(material.shader.name);
                            if (material.renderQueue != que)
                            {
                                material.renderQueue = que;
                            }
                        }
                    }
                }
            }
        
        
            // 天空盒shader
            if (RenderSettings.skybox != null && RenderSettings.skybox.shader != null)
            {
                RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
            }
        }
        
        InitPostProcess();
//        GameObject root = this.gameObject.transform.parent.parent.parent.parent.gameObject;
//        LightmapData[] lightmaps = root.GetComponentsInChildren<LightmapData>();
        Logger.LogColor(Color.green, $"light map data count {LightmapSettings.lightmaps.Length}");
    }

    void InitPostProcess()
    {
        // 设置后期处理
//        PostProcessLayer postLayer = FindObjectOfType<PostProcessLayer>();
//        PostProcessVolume postVolume = FindObjectOfType<PostProcessVolume>();
//        
//        
        KVQuality.ResetPostProcess(true);
    }
    
    
    
#endif
}
