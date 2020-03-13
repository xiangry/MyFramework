using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// added by wsh @ 2017.12.25
/// 注意：
/// 1、所有ab路径中目录、文件名不能以下划线打头，否则出包时StreamingAssets中的资源不能打到真机上，很坑爹
/// </summary>

namespace AssetBundles
{
    public class AssetBundleConfig
    {
        public const string localSvrAppPath = "Editor/AssetBundle/LocalServer/AssetBundleServer.exe";
        public const string AssetBundlesFolderName = "AssetBundles";
        public const string LuaScriptsFolderName = "LuaScripts";
        public const string AssetBundlesIndependentFolderName = "Independent";
        public const string AssetBundleSuffix = ".assetbundle";
        public const string AssetsFolderName = "AssetsPackage";
        public const string ChannelFolderName = "Channel";
        public const string AssetsPathMapFileName = "AssetsMap.bytes";
        public const string VariantsMapFileName = "VariantsMap.bytes";
        public const string AssetBundleServerUrlFileName = "AssetBundleServerUrl.txt";
        public const string VariantMapParttren = "Variant";
        public const string CommonMapPattren = ",";
        public const string AtlasRoot = "UI/Atlas/";

#if UNITY_EDITOR
        public static string AssetBundlesBuildOutputPath
        {
            get
            {
                string outputPath = Path.Combine(System.Environment.CurrentDirectory, AssetBundlesFolderName);
                UtilityGame.CheckDirAndCreateWhenNeeded(outputPath);
                return outputPath;
            }
        }

        public static string LocalSvrAppPath
        {
            get
            {
                return Path.Combine(Application.dataPath, localSvrAppPath);
            }
        }

        public static string LocalSvrAppWorkPath
        {
            get
            {
                return AssetBundlesBuildOutputPath;
            }
        }

        private static int mIsEditorMode = -1;
        private const string kIsEditorMode = "IsEditorMode";
        private static int mIsSimulateMode = -1;
        private const string kIsSimulateMode = "IsSimulateMode";
        private static int mIsSimulateMode_UseLuaScripts = -1;
        private const string kIsSimulateMode_UseLuaScripts = "IsSimulateMode_UseLuaScripts";
        private const string kIsPackLuaAB = "IsPackLuaAB";
        private static int mIsPackLuaAB = -1;


        public static bool IsEditorMode
        {
            get
            {
                if (mIsEditorMode == -1)
                {
                    if (!EditorPrefs.HasKey(kIsEditorMode))
                    {
                        EditorPrefs.SetBool(kIsEditorMode, false);
                    }
                    mIsEditorMode = EditorPrefs.GetBool(kIsEditorMode, true) ? 1 : 0;
                }

                return mIsEditorMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mIsEditorMode)
                {
                    mIsEditorMode = newValue;
                    EditorPrefs.SetBool(kIsEditorMode, value);
                    if (value)
                    {
                        IsSimulateMode = false;
                        IsSimulateMode_UseLuaScripts = false;
                    }
                }
            }
        }

        public static bool IsSimulateMode
        {
            get
            {
                if (mIsSimulateMode == -1)
                {
                    if (!EditorPrefs.HasKey(kIsSimulateMode))
                    {
                        EditorPrefs.SetBool(kIsSimulateMode, true);
                    }
                    mIsSimulateMode = EditorPrefs.GetBool(kIsSimulateMode, true) ? 1 : 0;
                }

                return mIsSimulateMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mIsSimulateMode)
                {
                    mIsSimulateMode = newValue;
                    EditorPrefs.SetBool(kIsSimulateMode, value);

                    if (value)
                    {
                        IsEditorMode = false;
                        IsSimulateMode_UseLuaScripts = false;
                    }
                }
            }
        }
        
        public static bool IsSimulateMode_UseLuaScripts
        {
            get
            {
                if (mIsSimulateMode_UseLuaScripts == -1)
                {
                    if (!EditorPrefs.HasKey(kIsSimulateMode_UseLuaScripts))
                    {
                        EditorPrefs.SetBool(kIsSimulateMode_UseLuaScripts, true);
                    }
                    mIsSimulateMode_UseLuaScripts = EditorPrefs.GetBool(kIsSimulateMode_UseLuaScripts, true) ? 1 : 0;
                }

                return mIsSimulateMode_UseLuaScripts != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mIsSimulateMode_UseLuaScripts)
                {
                    mIsSimulateMode_UseLuaScripts = newValue;
                    EditorPrefs.SetBool(kIsSimulateMode_UseLuaScripts, value);

                    if (value)
                    {
                        IsEditorMode = false;
                        IsSimulateMode = true;
                    }
                }
            }
        }
        
        
        public static bool IsPackLuaAb
        {
            get
            {
                if (mIsPackLuaAB == -1)
                {
                    if (!EditorPrefs.HasKey(kIsPackLuaAB))
                    {
                        EditorPrefs.SetBool(kIsPackLuaAB, false);
                    }
                    mIsPackLuaAB = EditorPrefs.GetBool(kIsPackLuaAB, true) ? 1 : 0;
                }

                return mIsPackLuaAB != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mIsPackLuaAB)
                {
                    mIsPackLuaAB = newValue;
                    EditorPrefs.SetBool(kIsPackLuaAB, value);
                }
            }
        }
        
#endif
    }
}