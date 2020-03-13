using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KV
{
    
    enum DebugSortType
    {
        LoadTime = 1,
        DataSize = 2,
        DataPerTime = 3,
    }
    
    public class AssetBundleLoadInfo
    {
        public string AssetPath;
        public float LoadTime;
        public int dataSize;
        public int LoadTimes;

        public AssetBundleLoadInfo(string path, float time, int dataSize)
        {
            AssetPath = path;
            LoadTime = time;
        }
    }

    public class AssetInfo
    {
        public string AssetPath;
        public string AssetName;

        public AssetInfo(string assetPath, string assetName)
        {
            if (assetPath == null)
            {
                assetPath = "null";
            }
            if (assetName == null)
            {
                assetName = "null";
            }
            AssetPath = assetPath;
            AssetName = assetName;
        }
    }
    
    public class KVDebugHelper : MonoSingleton<KVDebugHelper>
    {
        public List<AssetBundleLoadInfo> assetTimeList = new List<AssetBundleLoadInfo>();
        public float allLoadTime = 0f;
        
        public List<AssetInfo> assetInfoList = new List<AssetInfo>();

        public void AddAssetInfo(string path, string name)
        {
            assetInfoList.Add(new AssetInfo(path, name));
        }

        public void SortLoadAssetInfo()
        {
            assetInfoList.Sort((a, b) =>
            {
                return CompareStr(a.AssetName, b.AssetName);
            });
        }
        
        public void AddLoadTimeInfo(string path, float time, int dataSize)
        {
            int index = path.IndexOf("StreamingAssets");
            path = path.Substring(index + "StreamingAssets".Length);
            assetTimeList.Add(new AssetBundleLoadInfo(path, time, dataSize));

            allLoadTime += time;
            
//            FileStream x = new FileStream("xxxx", FileMode.Create);
            
            
            assetTimeList.Sort((a, b) =>
            {
                if (a.LoadTime < b.LoadTime)
                {
                    return 1;
                }
                else if(a.LoadTime > b.LoadTime)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });
        }
        
        public int CompareStr(string s1, string s2)
        {
            int num = Math.Min(s1.Length, s2.Length); //防止数组越过
            for (int i = 0; i < num; i++)
            {
                if (s1[i] > s2[i])
                {
                    return 1;
                }
                else if (s1[i] < s2[i])
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}