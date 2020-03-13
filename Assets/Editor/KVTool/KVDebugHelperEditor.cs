using System;
using UnityEditor;
using UnityEngine;


namespace KV
{
        [CustomEditor(typeof(KVDebugHelper), true)]
        public class AssetBundleManagerEditor : Editor
        {
                private bool loadFlag = false;
                private bool infoFlag = false;
                
                public override void OnInspectorGUI()
                {
                        base.OnInspectorGUI();
                        DebugToolGUI();
                        
                        var instance = KVDebugHelper.Instance;
                        EditorGUILayout.BeginVertical();


                        EditorGUILayout.TextField($"加载时间总计：{instance.allLoadTime:F4} 文件数：{instance.assetTimeList.Count}");
                        loadFlag = EditorGUILayout.Foldout(loadFlag, "加载Asset时间信息");
                        if (loadFlag)
                        {
                                foreach (var item in instance.assetTimeList)
                                {
                                        DrawAssetLoadTimeItem(item);
                                }
                        }
                        
                        
                        infoFlag = EditorGUILayout.Foldout(infoFlag, "加载的AssetInfo信息");
                        if (infoFlag)
                        {
                                if(GUILayout.Button("Sort"))
                                {
                                        instance.SortLoadAssetInfo();
                                }
                                foreach (var item in instance.assetInfoList)
                                {
                                        DrawAssetAssetInfoItem(item);
                                }
                        }
                        

                        EditorGUILayout.Separator();
                        
                        EditorGUILayout.EndVertical();
                }


                protected void DrawAssetLoadTimeItem(AssetBundleLoadInfo loadInfo)
                {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.TextField($"{loadInfo.LoadTime:F4}\t{loadInfo.dataSize}\t{loadInfo.AssetPath}");
                        EditorGUILayout.EndHorizontal();
                }
                
                protected void DrawAssetAssetInfoItem(AssetInfo assetInfo)
                {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.TextField($"{assetInfo.AssetName}\t{assetInfo.AssetPath}");
                        EditorGUILayout.EndHorizontal();
                }

                public void DebugToolGUI()
                {

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("清理数据", GUILayout.Width(100)))
                        {
                                ClearData();
                        }

                        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
                        {
                                SaveData();
                        }
                        EditorGUILayout.EndHorizontal();
                }


                void ClearData()
                {
                        KVDebugHelper.Instance.assetTimeList.Clear();
                        KVDebugHelper.Instance.allLoadTime = 0;
                        Debug.LogError($"清理数据");
                }
                
                void SaveData()
                {
                        Debug.LogError($"保存数据");
                }

        }

}