using UnityEngine;
using XLua;
using System.IO;
using UnityEditor;

/// <summary>
/// added by wsh @ 2017.12.25
/// 功能： Assetbundle相关的通用静态函数，提供运行时，或者Editor中使用到的有关Assetbundle操作和路径处理的函数
/// TODO：
/// 1、做路径处理时是否考虑引入BetterStringBuilder消除GC问题
/// 2、目前所有路径处理不支持variant，后续考虑是否支持
/// </summary>

namespace AssetBundles
{
    [Hotfix]
    [LuaCallCSharp]
    public class AssetBundleUtility
    {
        private static string GetPlatformName(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    Logger.LogError("Error platform!!!");
                    return null;
            }
        }
       
        public static string GetStreamingAssetsFilePath(string assetPath = null, bool isIndependent = false)
        {
            string folderName;
            if (isIndependent)
            {
                folderName = AssetBundleConfig.AssetBundlesIndependentFolderName;
            }
            else
            {
                folderName = AssetBundleConfig.AssetBundlesFolderName;
            }
#if UNITY_EDITOR
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, folderName);
#else
#if UNITY_IPHONE || UNITY_IOS
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, folderName);
#elif UNITY_ANDROID
            string outputPath = Path.Combine(Application.streamingAssetsPath, folderName);
#else
            Logger.LogError("Unsupported platform!!!");
#endif
#endif
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
        }

        public static string GetStreamingAssetsDataPath(string assetPath = null, bool isIndependent = false)
        {
            string folderName;
            if (isIndependent)
            {
                folderName = AssetBundleConfig.AssetBundlesIndependentFolderName;
            }
            else
            {
                folderName = AssetBundleConfig.AssetBundlesFolderName;
            }
            string outputPath = Path.Combine(Application.streamingAssetsPath, folderName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
        }

        public static string GetPersistentFilePath(string assetPath = null, bool isIndependent = false)
        {
            return "file://" + GetPersistentDataPath(assetPath, isIndependent);
        }

        public static string GetPersistentDataPath(string assetPath = null, bool isIndependent = false)
        {
            string folderName;
            if (isIndependent)
            {
                folderName = AssetBundleConfig.AssetBundlesIndependentFolderName;
            }
            else
            {
                folderName = AssetBundleConfig.AssetBundlesFolderName;
            }
            
//            string outputPath = Path.Combine(Application.persistentDataPath, folderName);
            string outputPath = Core.Resource.FileUtil.GetWritePath(folderName);  //修改了写目录
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
#if UNITY_EDITOR
            return UtilityGame.FormatToSysFilePath(outputPath);
#else
            return outputPath;
#endif
        }

        public static bool CheckPersistentFileExsits(string filePath, bool Independent = false)
        {
            var path = GetPersistentDataPath(filePath, Independent);
            return File.Exists(path);
        }

        // 注意：这个路径是给WWW读文件使用的url，如果要直接磁盘写persistentDataPath，使用GetPlatformPersistentDataPath
        public static string GetAssetBundleFileUrl(string filePath)
        {
            if (CheckPersistentFileExsits(filePath))
            {
                return GetPersistentFilePath(filePath);
            }
            else if(CheckPersistentFileExsits(filePath, true))
            {
                return GetPersistentFilePath(filePath, true);
            }
            else
            {
//                if (File.Exists(GetStreamingAssetsFilePath(filePath, true)))
//                {
//                    return GetStreamingAssetsFilePath(filePath, true);
//                }
//                else
//                {
                    return GetStreamingAssetsFilePath(filePath);
//                }
            }
        }
        
        // 注意：这个路径是给WWW读文件使用的url，如果要直接磁盘写persistentDataPath，使用GetPlatformPersistentDataPath
        public static string GetIndependentAssetBundleFileUrl(string filePath)
        {
            if(CheckPersistentFileExsits(filePath, true))
            {
                return GetPersistentFilePath(filePath, true);
            }
            else
            {
                return GetStreamingAssetsFilePath(filePath, true);
            }
        }
        
        public static string AssetBundlePathToAssetBundleName(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (assetPath.StartsWith("Assets/"))
                {
                    assetPath = AssetsPathToPackagePath(assetPath);
                }
                //no " "
                assetPath = assetPath.Replace(" ", "");
                //there should not be any '.' in the assetbundle name
                //otherwise the variant handling in client may go wrong
                assetPath = assetPath.Replace(".", "_");
                //add after suffix ".assetbundle" to the end
                assetPath = assetPath + AssetBundleConfig.AssetBundleSuffix;
                return assetPath.ToLower();
            }
            return null;
        }
        
        public static string PackagePathToAssetsPath(string assetPath)
        {
            return "Assets/" + AssetBundleConfig.AssetsFolderName + "/" + assetPath;
        }

        public static bool IsPackagePath(string assetPath)
        {
            string path = "Assets/" + AssetBundleConfig.AssetsFolderName + "/";
            return assetPath.StartsWith(path);
        }
        
        public static string AssetsPathToPackagePath(string assetPath)
        {
            string path = "Assets/" + AssetBundleConfig.AssetsFolderName + "/";
            if (assetPath.StartsWith(path))
            {
                return assetPath.Substring(path.Length);
            }
            else
            {
                Debug.LogError("Asset path is not a package path!");
                return assetPath;
            }
        }
    }
}