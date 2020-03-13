﻿using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2018.01.03
/// 说明：Assetbundle检测器，由于Unity中的AssetBundle名字标签很不好管理，这里做一层检测以防漏
/// 注意：
/// 1、每个Assetbundle对应配置一个Checker，Checker对应的PackagePath及为Assetbundle所在路径
/// 2、每个Checker可以检测多个目录或者文件，这些目录或者文件被打入一个Assetbundle包
/// TODO：
/// 1、提供自动化的Checker，每次检测到有Asset变动（移动、新增、删除）时自动Check
/// 2、提供一套可视化编辑界面，将Checker配置化并展示到Inspector，从而新增/删除Checker不需要写代码
/// 3、支持Variant
/// </summary>

namespace AssetBundles
{
    public class AssetBundleCheckerFilter
    {
        public string RelativePath;
        public string ObjectFilter;

        public AssetBundleCheckerFilter(string relativePath, string objectFilter)
        {
            RelativePath = relativePath;
            ObjectFilter = objectFilter;
        }
    }

    public class AssetBundleCheckerConfig
    {
        public string PackagePath = string.Empty;
        public List<AssetBundleCheckerFilter> CheckerFilters = null;

        public AssetBundleCheckerConfig()
        {
        }

        public AssetBundleCheckerConfig(string packagePath, List<AssetBundleCheckerFilter> checkerFilters)
        {
            PackagePath = packagePath;
            CheckerFilters = checkerFilters;
        }
    }

    public class AssetBundleChecker
    {
        string assetsPath;
        AssetBundleImporter importer;
        AssetBundleCheckerConfig config;

        public AssetBundleChecker(AssetBundleCheckerConfig config)
        {
            this.config = config;
            assetsPath = AssetBundleUtility.PackagePathToAssetsPath(config.PackagePath);
            importer = AssetBundleImporter.GetAtPath(assetsPath);
            if (importer == null || !importer.IsValid)
            {
                Logger.LogError($"定义的Package信息path:({assetsPath}) 没有找不到，请对比Asset/Editor/AssetBundle/xxx和Asset/AssetsPackage/xxx");
            }
        }

        public void CheckAssetBundleName()
        {
            if (importer == null || !importer.IsValid)
            {
                return;
            }

            var checkerFilters = config.CheckerFilters;
            if (checkerFilters == null || checkerFilters.Count == 0)
            {
                importer.assetBundleName = assetsPath;
            }
            else
            {
                foreach (var checkerFilter in checkerFilters)
                {
                    var relativePath = assetsPath;
                    if (!string.IsNullOrEmpty(checkerFilter.RelativePath))
                    {
                        relativePath = Path.Combine(assetsPath, checkerFilter.RelativePath);
                    }
                    var imp = AssetBundleImporter.GetAtPath(relativePath);
                    if (imp == null)
                    {
                        continue;
                    }
                    if (imp.IsFile)
                    {
                        importer.assetBundleName = assetsPath;
                        continue;
                    }
                    string[] objGuids = AssetDatabase.FindAssets(checkerFilter.ObjectFilter, new string[] { relativePath });
                    foreach (var guid in objGuids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        imp = AssetBundleImporter.GetAtPath(path);
                        imp.assetBundleName = assetsPath;
                    }
                }
            }
        }

        ///// <summary>
        ///// 得到合适的FairyGUI AssetBundleName
        ///// </summary>
        public void CheckFairyGUIAssetBundleName()
        {
            if (!importer.IsValid)
            {
                return;
            }
            var checkerFilters = config.CheckerFilters;
            if (checkerFilters == null || checkerFilters.Count == 0)
            {
                importer.assetBundleName = assetsPath;
            }
            else
            {
                foreach (var checkerFilter in checkerFilters)
                {
                    var relativePath = assetsPath;
                    if (!string.IsNullOrEmpty(checkerFilter.RelativePath))
                    {
                        relativePath = Path.Combine(assetsPath, checkerFilter.RelativePath);
                    }
                    var imp = AssetBundleImporter.GetAtPath(relativePath);
                    if (imp == null)
                    {
                        continue;
                    }
                    if (imp.IsFile)
                    {
                        int position = relativePath.LastIndexOf(".");
                        relativePath = position > -1 ? relativePath.Substring(0, position) : relativePath;

                        int isResIndex = relativePath.IndexOf("@");
                        if (isResIndex > -1)
                            relativePath = relativePath.Substring(0, isResIndex) + "_res";
                        //Debug.Log(">>>>>>>>>>:" + relativePath);
                    }
                    importer.assetBundleName = relativePath;
                }
            }
        }




        public void CheckChannelName()
        {
            string channelAssetPath = Path.Combine(AssetBundleConfig.ChannelFolderName, config.PackagePath);
            channelAssetPath = AssetBundleUtility.PackagePathToAssetsPath(channelAssetPath) + ".bytes";
            if (!File.Exists(channelAssetPath))
            {
                UtilityGame.SafeWriteAllText(channelAssetPath, "None");
                AssetDatabase.Refresh();
            }

            var imp = AssetBundleImporter.GetAtPath(channelAssetPath);
            imp.assetBundleName = assetsPath;
        }

        public static void Run(AssetBundleCheckerConfig config, bool checkChannel, bool isFairyGui = false)
        {
            var checker = new AssetBundleChecker(config);

            if (isFairyGui)
                checker.CheckFairyGUIAssetBundleName();
            else
                checker.CheckAssetBundleName();
            if (checkChannel)
            {
                checker.CheckChannelName();
            }
            AssetDatabase.Refresh();
        }
    }
}

