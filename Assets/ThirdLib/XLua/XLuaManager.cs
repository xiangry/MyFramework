﻿using System;
using AssetBundles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;

/// <summary>
/// 说明：xLua管理类
/// 注意：
/// 1、整个Lua虚拟机执行的脚本分成3个模块：热修复、公共模块、逻辑模块
/// 2、公共模块：提供Lua语言级别的工具类支持，和游戏逻辑无关，最先被启动
/// 3、热修复模块：脚本全部放Lua/XLua目录下，随着游戏的启动而启动
/// 4、逻辑模块：资源热更完毕后启动
/// 5、资源热更以后，理论上所有被加载的Lua脚本都要重新执行加载，如果热更某个模块被删除，则可能导致Lua加载异常，这里的方案是释放掉旧的虚拟器另起一个
/// @by wsh 2017-12-28
/// </summary>

[Hotfix]
[LuaCallCSharp]
public class XLuaManager : MonoSingleton<XLuaManager>
{
        public const string luaAssetbundleAssetName = "Lua";
        public const string luaScriptsFolder = @"../LuaScripts";
        const string commonMainScriptName = "Common.Main";
        const string gameMainScriptName = "GameMain";
        const string hotfixMainScriptName = "XLua.HotfixMain";
        LuaEnv luaEnv = null;
        LuaUpdater luaUpdater = null;
        const string gameTextTranslateFuncName = "__";
        private LuaFunction translateTextFunc = null;

        protected override void Init()
        {
                base.Init();
                string path = AssetBundleUtility.PackagePathToAssetsPath(luaAssetbundleAssetName);
                AssetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(path);
                InitLuaEnv();
                SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
        }

        public bool HasGameStart
        {
                get;
                protected set;
        }

        public LuaEnv GetLuaEnv()
        {
                return luaEnv;
        }

        void InitLuaEnv()
        {
                luaEnv = new LuaEnv();
                HasGameStart = false;
                if (luaEnv != null)
                {
                        luaEnv.AddLoader(CustomLoader);
                        //luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPb);
                }
                else
                {
                        Logger.LogError("InitLuaEnv null!!!");
                }
        }

        // 这里必须要等待资源管理模块加载Lua AB包以后才能初始化
        public void OnInit()
        {
                if (luaEnv != null)
                {
                        LoadScript(commonMainScriptName);
                        luaUpdater = gameObject.GetComponent<LuaUpdater>();
                        if (luaUpdater == null)
                        {
                                luaUpdater = gameObject.AddComponent<LuaUpdater>();
                        }
                        luaUpdater.OnInit(luaEnv);
                }
        }

        public string AssetbundleName
        {
                get;
                protected set;
        }

        // 重启虚拟机：热更资源以后被加载的lua脚本可能已经过时，需要重新加载
        // 最简单和安全的方式是另外创建一个虚拟器，所有东西一概重启
        public void Restart()
        {
                StopHotfix();
                Dispose();
                InitLuaEnv();
                OnInit();
        }

        public object[] SafeDoString(string scriptContent)
        {
                if (luaEnv != null)
                {
                        try
                        {
                                luaEnv.DoString(scriptContent);
                        }
                        catch (System.Exception ex)
                        {
                                string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                                Logger.LogError(msg, null);
                        }
                }
                return null;
        }

        public void StartHotfix(bool restart = false)
        {
                if (luaEnv == null)
                {
                        return;
                }

                if (restart)
                {
                        StopHotfix();
                        ReloadScript(hotfixMainScriptName);
                }
                else
                {
                        LoadScript(hotfixMainScriptName);
                }
                SafeDoString("HotfixMain.Start()");
        }

        public void StopHotfix()
        {
                SafeDoString("HotfixMain.Stop()");
        }

        public void StartGame()
        {
                if (luaEnv != null)
                {
                        LoadScript(gameMainScriptName);
                        SafeDoString("GameMain.Start()");
                        HasGameStart = true;
                }
        }

        public void ReloadScript(string scriptName)
        {
                SafeDoString(string.Format("package.loaded['{0}'] = nil", scriptName));
                LoadScript(scriptName);
        }

        void LoadScript(string scriptName)
        {
                SafeDoString(string.Format("require('{0}')", scriptName));
        }

        public static byte[] CustomLoader(ref string filepath)
        {
                string scriptPath = string.Empty;
                filepath = filepath.Replace(".", "/") + ".lua";
                scriptPath = "LuaScripts/" + filepath;

#if UNITY_EDITOR
                if (AssetBundleConfig.IsEditorMode || AssetBundleConfig.IsSimulateMode_UseLuaScripts)
                {
                        scriptPath = Path.Combine(Application.dataPath, luaScriptsFolder);
                        scriptPath = Path.Combine(scriptPath, filepath);
                        //Logger.Log("Load lua script : " + scriptPath);
                        return UtilityGame.SafeReadAllBytes(scriptPath);
                }
#endif
                //优先读取可写目录文件        
                string realPath = FileUtil.GetWritePath(scriptPath);
                if (File.Exists(realPath))
                {
                        return UtilityGame.SafeReadAllBytes(realPath);
                }
                
#if UNITY_ANDROID && ! UNITY_EDITOR
                byte[] bytes = AndroidAssetLoadSDK.LoadFile(scriptPath);
                if (bytes != null)
                {
                        return bytes;
                }
#else
                //优先读取可写目录文件        
                realPath = FileUtil.GetReadOnlyPath(scriptPath);
                if (File.Exists(realPath))
                {
                        return UtilityGame.SafeReadAllBytes(realPath);
                }
#endif
                Logger.LogError($"Lua File Not Found:({scriptPath})");
                
                
//                scriptPath = string.Format("{0}/{1}.bytes", luaAssetbundleAssetName, filepath);
//                string assetbundleName = null;
//                string assetName = null;
//                bool status = AssetBundleManager.Instance.MapAssetPath(scriptPath, out assetbundleName, out assetName);
//                if (!status)
//                {
//                        Logger.LogError("MapAssetPath failed : " + scriptPath);
//                        return null;
//                }
//                var asset = AssetBundleManager.Instance.GetAssetCache(assetName) as TextAsset;
//                if (asset != null)
//                {
//                        //Logger.Log("Load lua script : " + scriptPath);
//                        return asset.bytes;
//                }
//                Logger.LogError("Load lua script failed : " + scriptPath + ", You should preload lua assetbundle first!!!");
                return null;
        }
        
        /// <summary>
        /// 获取lua中多语言转换的方法
        /// </summary>
        /// <returns>LuaFunction</returns>
        public LuaFunction GetTranslateTextFunc()
        {
                if (translateTextFunc == null)
                {
                        translateTextFunc = luaEnv.Global.Get<LuaFunction>(gameTextTranslateFuncName);
                }

                return translateTextFunc;
        }


        private void Update()
        {
                if (luaEnv != null)
                {
                        luaEnv.Tick();

                        if (Time.frameCount % 100 == 0)
                        {
                                luaEnv.FullGc();
                        }
                }
        }

        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
                if (luaEnv != null && HasGameStart)
                {
                        SafeDoString("GameMain.OnLevelWasLoaded()");
                }
        }

        private void OnApplicationQuit()
        {
                if (luaEnv != null && HasGameStart)
                {
                        SafeDoString("GameMain.OnApplicationQuit()");
                }
        }

        Dictionary<string, LuaFunction> luaFunctionList = new Dictionary<string, LuaFunction>();
        StringBuilder sb = new StringBuilder();
        public object[] CallLuaFunction(string module, string function, params object[] args)
        {
                LuaFunction func = null;
                sb.Clear();
                string fullName = function;
                if (module != null)
                        fullName = sb.AppendFormat("{0}.{1}", module, function).ToString();

                if (!luaFunctionList.TryGetValue(fullName, out func))
                {
                        LuaTable tempTable = luaEnv.Global;

                        if (module != null)
                        {
                                string[] tempModule = module.Split('.');
                                for (int i = 0; i < tempModule.Length; i++)
                                {
                                        var table = tempTable.Get<LuaTable>(tempModule[i]);
                                        if (table == null) return null;
                                        tempTable = table;
                                }
                                if (tempTable == null) return null;
                        }
                        func = tempTable.Get<LuaFunction>(function);
                        luaFunctionList.Add(fullName, func);
                }
                if (func != null)
                {
                        if (args.Length > 0)
                                return func.Call(args);
                        return func.Call();
                }

                return null;
        }

        public override void Dispose()
        {
                if (translateTextFunc != null)
                {
                        translateTextFunc.Dispose();
                        translateTextFunc = null;
                }
                
                SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
                if (luaUpdater != null)
                {
                        luaUpdater.OnDispose();
                }
                if (luaEnv != null)
                {
                        try
                        {
                                luaEnv.Dispose();
                                luaEnv = null;
                        }
                        catch (System.Exception ex)
                        {
                                string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                                Logger.LogError(msg, null);
                        }
                }
                luaFunctionList.Clear();
        }
}
