using System;
using System.Collections;
using AssetBundles;
using UnityEngine;
using Core.Resource;
using XLua;

[Hotfix()]
[LuaCallCSharp()]
[CSharpCallLua]
public class GameConfig : MonoBehaviour
{
    #region 单例
    private static GameConfig m_instance;
    public static GameConfig instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject obj = new GameObject("____GameConfig____");
                m_instance = obj.AddComponent<GameConfig>();
                DontDestroyOnLoad(obj);
                
                m_instance.Init();
            }

            return m_instance;
        }
    }

    public static void Release()
    {
        if (m_instance != null)
        {
            m_instance = null;
        }
    }

    #endregion
    
    
    public virtual void Init()
    {
#if (((UNITY_ANDROID || UNITY_IPHONE || UNITY_WINRT || UNITY_BLACKBERRY) && !UNITY_EDITOR))
            FileUtil.SetReadOnlyDirectory(Application.streamingAssetsPath + "/");
            FileUtil.SetWriteDirectory(Application.persistentDataPath + "/");
#else
//        string readOnlyDir = FileUtil.GetParentDir(Application.dataPath);
        FileUtil.SetReadOnlyDirectory(Application.streamingAssetsPath);
        string writeableDir = FileUtil.GetParentDir(Application.dataPath) + "/Documents/";
        FileUtil.SetWriteDirectory( writeableDir);
        Application.runInBackground = true;
#endif

    }
    
    public void Dispose()
    {
        
    }

    public string GetGrayPath()
    {
        return "";
    }
    
    

    public void ReloadGame(Action callback)
    {
        StartCoroutine(StartReloadGame(callback));
    }

    IEnumerator StartReloadGame(Action callback)
    {
        // 重启资源管理器
        yield return AssetBundleManager.Instance.Cleanup();
        yield return AssetBundleManager.Instance.Initialize();
   
        //i重启图集管理
        yield return Sword.SpriteAtlasManager.Instance.Reset();
        
        // 重启Lua虚拟机
//        string luaAssetbundleName = XLuaManager.Instance.AssetbundleName;
//        AssetBundleManager.Instance.SetAssetBundleResident(luaAssetbundleName, true);
//        var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync(luaAssetbundleName);
//        yield return abloader;
//        abloader.Dispose();
        callback();
        XLuaManager.Instance.Restart();
        XLuaManager.Instance.StartHotfix();
        
        XLuaManager.Instance.StartGame();
        CustomDataStruct.Helper.Startup();
        UINoticeTip.Instance.DestroySelf();
    }
}