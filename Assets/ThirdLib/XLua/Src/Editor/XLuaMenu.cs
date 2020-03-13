﻿using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Debug = UnityEngine.Debug;
using AssetBundles;

[InitializeOnLoad]
public static class XLuaMenu
{
    [MenuItem("XLua/Copy Lua Files To AssetsPackage", false, 51)]
    public static void CopyLuaFilesToAssetsPackage()
    {
        string destination = Path.Combine(Application.dataPath, AssetBundleConfig.AssetsFolderName);
        destination = Path.Combine(destination, XLuaManager.luaAssetbundleAssetName);
        string source = Path.Combine(Application.dataPath, XLuaManager.luaScriptsFolder);
        UtilityGame.SafeDeleteDir(destination);


//        if (AssetBundleConfig.IsPackLuaAb == false)
//        {
//            Directory.CreateDirectory(destination);
//            Logger.Log("------------------------------------ 没有拷贝lua目录");
//            FileStream f = File.OpenWrite(destination + "/main.lua.bytes");
//            var str = "print('test')";
//            f.Write( System.Text.Encoding.Default.GetBytes(str), 0, str.Length);
//            f.Close();
//            AssetDatabase.Refresh();
//            return;
//        }

        FileUtil.CopyFileOrDirectoryFollowSymlinks(source, destination);

        var notLuaFiles = UtilityGame.GetSpecifyFilesInFolder(destination, new string[] { ".lua" }, true);
        if (notLuaFiles != null && notLuaFiles.Length > 0)
        {
            for (int i = 0; i < notLuaFiles.Length; i++)
            {
                UtilityGame.SafeDeleteFile(notLuaFiles[i]);
            }
        }

        var luaFiles = UtilityGame.GetSpecifyFilesInFolder(destination, new string[] { ".lua" }, false);
        if (luaFiles != null && luaFiles.Length > 0)
        {
            for (int i = 0; i < luaFiles.Length; i++)
            {
                UtilityGame.SafeRenameFile(luaFiles[i], luaFiles[i] + ".bytes");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Copy lua files over");
    }
}
