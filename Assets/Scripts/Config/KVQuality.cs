using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class KVQuality
{
    // 首次进入游戏，会根据机型设置一个基本的Quality
    
    public const string GameQualityLevel = "GameQualityLevel";
    public const string PlayerShader = "PlayerShader";
    public const string PlayerOutLine = "PlayerOutLine";
    public const string PostProcessing = "PostProcessing";
    public const string DynamicShadows = "DynamicShadows";
    public const string AntiAliasing = "AntiAliasing";
    public const string EffectLod = "EffectLod";
    public const string HighFrameRate = "HighFrameRate";
    
    public int quality = -1;

    public static void InitQuality()
    {
        UpdateHighFrame();
    }
    
    public static bool IsOpenEffectByKey(string key)
    {
        // 无数据，默认开启功能
        return UnityEngine.PlayerPrefs.GetInt(key, 1) == 1;
    }

    public static void UpdateHighFrame()
    {
        bool isHighFrame = KVQuality.IsOpenEffectByKey(KVQuality.HighFrameRate);
        int vSyncCount = 2;
        int frameRate = 30;
        if (isHighFrame)
        {
            frameRate = 60;
            vSyncCount = 1;
        }

        if (QualitySettings.vSyncCount != vSyncCount)
        {
            QualitySettings.vSyncCount = vSyncCount;
        }
        if (Application.targetFrameRate == frameRate)
        {
            Application.targetFrameRate = vSyncCount;
        }
    }


    /// <summary>
    /// 后处理设置
    /// 此处会做初始化（isINitPost=ture or postLayer.enabled
    /// 此处会设置FXAA
    /// </summary>
    /// <param name="isInitPost"></param>
    public static void ResetPostProcess(bool isInitPost = false)
    {
        bool isOpenPostProcess = KVQuality.IsOpenEffectByKey(KVQuality.PostProcessing);
        bool isOpenAntiAliasing = KVQuality.IsOpenEffectByKey(KVQuality.AntiAliasing);
        
        PostProcessResources postRes = Resources.Load<PostProcessResources>("PostProcessResources");
        PostProcessLayer[] postLayers = GameObject.FindObjectsOfType(typeof(PostProcessLayer)) as PostProcessLayer[];
        foreach (var postLayer in postLayers)
        {
            bool isLastEnable = postLayer.enabled;
            
            postLayer.enabled = isOpenPostProcess;
            PostProcessVolume volume = postLayer.gameObject.GetComponent<PostProcessVolume>();
            if (volume)
            {
                volume.enabled = isOpenPostProcess;
            }
            

            if (isOpenPostProcess)
            {
                if (isInitPost || (! isLastEnable))
                {
                    postLayer.Init(postRes);
                }

                if (isOpenAntiAliasing)        //开启fxAA移动级别抗锯齿
                {
                    postLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                    postLayer.fastApproximateAntialiasing.fastMode = true;
                    postLayer.fastApproximateAntialiasing.keepAlpha = true;
                }
            }
        }
    }
    
    
}
