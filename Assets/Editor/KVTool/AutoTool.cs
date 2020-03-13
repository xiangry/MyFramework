using System;
using System.Diagnostics;
using AssetBundles;
using GameChannel;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class AutoTool
{
    static void LinsteCMD()
    {
        var index = 0;
        foreach (string var in System.Environment.GetCommandLineArgs())
        {
            index++;
            Debug.LogError($"get cmd params {index}:{var}");
        }
    }

    static void PackAssetBundle()
    {
        Debug.LogError("打包资源 ----- 1");
        var start = DateTime.Now;
        Debug.LogError("打包资源 ----- 2");
        PackageTool.BuildAssetBundlesForCurrentChannel();
        Debug.LogError("打包资源 ----- 3");
        AssetBundleMenuItems.ToolsCopyAssetbundles();
        Debug.LogError("打包资源 ----- 4");
        AssetDatabase.Refresh();
        Debug.LogError("打包资源 ----- 5");
        Debug.LogError($"打包完成\nRunAllCheckres; ExecuteBuild; CopyToStreamingAsset; " +
                       $"使用时长为：{(DateTime.Now - start).TotalSeconds}");
        Debug.LogError("打包资源 ----- 6");
    }


    static void PackAndroidApp()
    {
        Debug.LogError("android package ----- 1");
        var start = DateTime.Now;

        Debug.LogError("android package ----- 2");
        BuildTarget buildTarget = BuildTarget.Android;
        ChannelType channelType = ChannelType.Test;
        Debug.LogError("android package ----- 3");

        AssetBundleDispatcherInspector.hasAnythingModified = true;
        BuildPlayer.BuildAssetBundles(buildTarget, channelType.ToString());
        AssetBundleDispatcherInspector.hasAnythingModified = false;
        Debug.LogError("android package ----- 4");

        AssetBundleMenuItems.ToolsCopyAssetbundlesAndScripts();
        Debug.LogError("android package ----- 5");

        PackageTool.BuildAndroidPlayerForCurrentChannel();
        Debug.LogError("android package ----- 6");

        var folder = PackageUtils.GetChannelOutputPath(buildTarget, channelType.ToString());
        EditorUtils.ExplorerFolder(folder);
        Debug.LogError("android package ----- 7");

        Debug.LogError($"Android Package Success!!! Use Time {(DateTime.Now - start).TotalSeconds}");
    }
}