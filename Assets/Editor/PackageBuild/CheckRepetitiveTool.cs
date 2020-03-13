using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using GameChannel;
using AssetBundles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine.Networking;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// added by wsh @ 2018.01.03
/// 说明：打包工具
/// TODO：
/// 1、
/// </summary>


public class CheckRepetitiveTool : EditorWindow
{
    public struct repetitiveInfo //每个重复资源的信息
    {   
        public UnityEngine.Object obj;
        public string path;
        public string bitmap;
        public List<UnityEngine.Object> useed_path_list;
        public List<repetitiveInfo> repetitive_list;
    }
    static private UnityEngine.Object targetObject = null;
    static private List<string> allTexbit = new List<string>();
    static private List<string> filePaths = new List<string>();
    static private List<repetitiveInfo> allTargetInfos = new List<repetitiveInfo>();

    static private bool isChecking = false;//是否正在检测
    static private bool foldoutType = false; //是否该刷新界面
    static private string[] ImageType = null;
    static private Vector2 scrollPos = new Vector2(0, 0);
    static private List<bool> allfoldouttype = new List<bool>();
    static private List<repetitiveInfo> all_need_find_list = new List<repetitiveInfo>();

    [MenuItem("Tools/图片查重", false, 0)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CheckRepetitiveTool));
    }

    void OnEnable()
    {
        string imgtype = "*.BMP|*.JPG|*.GIF|*.PNG";
        ImageType = imgtype.Split('|');
        targetObject = null;
    }
    private void OnDisable()
    {
        allTargetInfos.Clear();
    }
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.Label("拖拽一个文件夹或者一个图片到下面，如果不放置目标则遍历AssetsPackage下所有图片进行查重，时间花费巨大");
        GUILayout.BeginHorizontal();
        targetObject = EditorGUILayout.ObjectField(targetObject, typeof(UnityEngine.Object), true) as UnityEngine.Object;
        if (GUILayout.Button("开始检测", GUILayout.Width(200)))
        {           
            CheckBegin();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);
        if (allTargetInfos.Count > 0 && !isChecking)//&& dofresh
        {            
            for (int i = 0; i < allTargetInfos.Count; i++)
            {               
                ShowRepetitive(allTargetInfos[i], i + 1);
            }
        }
        GUILayout.EndScrollView();  
      EditorUtility.ClearProgressBar();
    }

    //展示检测出的数据
    void ShowRepetitive(repetitiveInfo target_info,int index)
    {       
        GUILayout.Space(10);
        string use_txt = target_info.useed_path_list.Count == 0 ? " 没有引用" : " ";
        string repetitive_txt = target_info.repetitive_list.Count == 0 ? " " : " 有重复资源";
        allfoldouttype[index - 1] = EditorGUILayout.Foldout(allfoldouttype[index - 1], "目标资源：" + target_info.obj.name
            + use_txt + repetitive_txt);
        if (allfoldouttype[index - 1])
        {
            GUILayout.BeginVertical();
                            
             EditorGUILayout.ObjectField(target_info.obj, typeof(UnityEngine.Object), true);
               
            if (target_info.useed_path_list.Count == 0) //没有引用
            {
                GUILayout.Label("该资源没有被引用", GUILayout.Width(150));
            }
            else
            {  
                GUILayout.Label("该目标资源存在的引用： ", GUILayout.Width(150));
                for (int j = 0; j < target_info.useed_path_list.Count; j++)
                {
                    GUILayout.BeginHorizontal();                   
                    GUILayout.Label("  "+(j + 1) + ".", GUILayout.Width(20));
                    EditorGUILayout.ObjectField(target_info.useed_path_list[j], typeof(UnityEngine.Object), false);
                    GUILayout.EndHorizontal();
                }
            }

            for (int i = 0; i < target_info.repetitive_list.Count; i++)
            {
                repetitiveInfo now_info = target_info.repetitive_list[i];
               
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("重复资源 NO"+(i+1), GUILayout.Width(150));
                    EditorGUILayout.ObjectField(now_info.obj, typeof(UnityEngine.Object), true);
                    GUILayout.EndHorizontal();
                    GUILayout.Label("该重复资源存在的引用： ", GUILayout.Width(150));
                    for (int j = 0; j < now_info.useed_path_list.Count; j++)
                    {
                        GUILayout.BeginHorizontal();                      
                        GUILayout.Label("  "+(j + 1) + ".", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(now_info.useed_path_list[j], typeof(UnityEngine.Object), false);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();                
            }            
            GUILayout.EndVertical();
        }         
    }


    //开始检测
    void CheckBegin()
    {
        isChecking = true;
        allTargetInfos.Clear();
        all_need_find_list.Clear();
        load();
        if (targetObject == null)
        {                      
            for (int i = 0; i < filePaths.Count; i++)
            {
                byte[] bytes = getImageByte(filePaths[i]);
                string bitmap = System.Convert.ToBase64String(bytes);
                repetitiveInfo target;
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePaths[i]);
                target.obj = obj;
                target.repetitive_list = new List<repetitiveInfo>();
                target.useed_path_list = new List<UnityEngine.Object>();//Find(filePaths[i]);
                target.path = filePaths[i];
                target.bitmap = bitmap;
                allTargetInfos.Add(target);
                all_need_find_list.Add(target);
            }
        }
        else if (targetObject.GetType() == typeof(Texture2D))
        {
            string first_path = AssetDatabase.GetAssetPath(targetObject);        
            byte[] bytes = getImageByte(first_path);
            string bitmap = System.Convert.ToBase64String(bytes);
            repetitiveInfo target;
            target.obj = targetObject;
            target.repetitive_list = new List<repetitiveInfo>();
            target.useed_path_list = new List<UnityEngine.Object>();// Find(first_path);
            target.path = first_path;
            target.bitmap = bitmap;
            allTargetInfos.Add(target);
            all_need_find_list.Add(target);
        }
        else if (targetObject.GetType() == typeof(DefaultAsset))
        {        
            string asset_path = AssetDatabase.GetAssetPath(targetObject);         
            for (int i = 0; i < ImageType.Length; i++)
            {
                string[] dirs = Directory.GetFiles(asset_path, ImageType[i], SearchOption.AllDirectories);
                for (int j = 0; j < dirs.Length; j++)
                {
                    string a = dirs[j];
                    string b = a.Replace(@"\", @"/");
                    byte[] bytes = getImageByte(b);
                    string bitmap = System.Convert.ToBase64String(bytes);
                    repetitiveInfo target;
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(b);                       
                    target.obj = obj;
                    target.repetitive_list = new List<repetitiveInfo>();
                    target.useed_path_list = new List<UnityEngine.Object>(); //Find(b);
                    target.path = b;
                    target.bitmap = bitmap;
                    allTargetInfos.Add(target);
                    all_need_find_list.Add(target);
                }
            }
        }
        CheckAllAssetsTexture();
    }
    void load()
    {
        allTexbit.Clear();
        filePaths.Clear();
        string imgtype = "*.BMP|*.JPG|*.GIF|*.PNG";
        string[] ImageType = imgtype.Split('|');
        for (int i = 0; i < ImageType.Length; i++)
        {
            //获取Application.dataPath文件夹下所有的图片路径  
            //Application.dataPath
            string[] dirs = Directory.GetFiles(("Assets" + "/AssetsPackage"), ImageType[i], SearchOption.AllDirectories);//
            for (int j = 0; j < dirs.Length; j++)
            {
                string a = dirs[j];        
                string b = a.Replace(@"\", @"/");                
                filePaths.Add(b);
                string bit = System.Convert.ToBase64String(getImageByte(b));
                allTexbit.Add(bit);
            }
        }
        //for (int i = 0; i < filePaths.Count; i++)
        //{            
           
        //}
    }
    /// <summary>  
    /// 根据图片路径返回图片的字节流byte[]  
    /// </summary>  
    /// <param name="imagePath">图片路径</param>  
    /// <returns>返回的字节流</returns>  
    private static byte[] getImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, System.IO.FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }

    //遍历整个工程的图片
    void CheckAllAssetsTexture()
    {        
        for (int i = 0; i < allTargetInfos.Count; i++)
        {            
            allfoldouttype.Add(false);
            repetitiveInfo target_info = allTargetInfos[i];
            isChecking = EditorUtility.DisplayCancelableProgressBar("匹配资源中", target_info.obj.name, i / allTargetInfos.Count);
            for (int j = 0; j < allTexbit.Count; j++)
            {                
                if (target_info.bitmap.Equals(allTexbit[j]) && !target_info.path.Equals(filePaths[j]))
                {
#if UNITY_EDITOR
                    UnityEngine.Object assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePaths[j]);
                    if (null != assetObj)
                    {
                        repetitiveInfo info;
                        info.obj = assetObj;
                        info.bitmap = allTexbit[j];
                        info.path = filePaths[j];
                        info.useed_path_list = new List<UnityEngine.Object>();//Find(info.path);
                        info.repetitive_list = null;
                        target_info.repetitive_list.Add(info);
                        all_need_find_list.Add(info);
                    }
#endif
                }
                else
                {
                    
                }
            }
        }
        FindUse();
        EditorUtility.ClearProgressBar();
        isChecking = false;
        
        OnGUI();
    }
    void FindUse()
    {
        List<UnityEngine.Object> list = new List<UnityEngine.Object>();        
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(Application.dataPath + "", "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;
        for (int i = startIndex; i < files.Length; i++)
        {
            string file = files[i];
            string alltext = File.ReadAllText(file);
            for (int j = 0; j < all_need_find_list.Count; j++)
            {
                string guid = AssetDatabase.AssetPathToGUID(all_need_find_list[j].path);
                if (Regex.IsMatch(alltext, guid))
                {
                    //找到引用                    
                    all_need_find_list[j].useed_path_list.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
                }
            }                       
        }        
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }



}