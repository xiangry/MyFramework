using System;
using UnityEngine;
using System.Collections;

public class AndroidAssetLoadSDK 
{
    public static byte[] LoadFile(string path)
    {
        try
        {
            AndroidJavaClass m_AndroidJavaClass = new AndroidJavaClass("com.ihaiu.assetloadsdk.AssetLoad");        
//        AndroidJavaClass m_AndroidJavaClass = new AndroidJavaClass("com.kvgame.AssetLoad");        
            Byte[] bytes = m_AndroidJavaClass.CallStatic<byte[]>("loadFile", path);
            return bytes;
        }
        catch (Exception e)
        {
            Logger.LogError($"LoadFile by Android failed {path}");
            throw;
        }
    }

    public static string LoadTextFile(string path)
    {
        byte[] bytes = LoadFile(path);
        if (bytes == null)
            return "Error bytes=null";
        
        return System.Text.Encoding.UTF8.GetString ( bytes );
    }

    public static AssetBundle LoadAssetBundle(string path)
    {
        byte[] bytes = LoadFile(path);
        if (bytes == null)
            return null;
        
        return AssetBundle.LoadFromMemory(bytes);
    }
}