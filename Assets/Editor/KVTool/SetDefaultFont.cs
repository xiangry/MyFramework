using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class SetDefaultFont : EditorWindow
{
    private static string m_fontPath;
    private static EditorWindow window;
    private static Action m_doneAction;
    private static Font m_font;
    
    [MenuItem("Tools/设置默认字体")]
    public static void OpenSetFontWindow()
    {
        OpenWindow();
    }

    public static void OpenWindow(Action aciton = null)
    {
        window = GetWindow(typeof (SetDefaultFont));
        window.minSize = new Vector2(500, 300);
        m_fontPath = ToolCacheManager.GetFontPath();
        m_font = ToolCacheManager.GetFont();
        m_doneAction = aciton;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("选择默认字体");
        EditorGUILayout.Space();
//        EditorGUILayout.ObjectField(m_font, typeof (Font), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"默认字体路径：{m_fontPath}");
        
        EditorGUILayout.Space();
        if (GUILayout.Button("选择默认字体"))
        {
            SelectFontFile();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("保存"))
        {
            ToolCacheManager.SaveFont(m_fontPath);
            window.Close();
            if (m_doneAction != null)
            {
                m_doneAction();
            }
        }
    }
    
    /// <summary>
    /// 选择生成工具目录
    /// </summary>
    private void SelectFontFile()
    {
        string[] filters = new[] {"FontFiles", "ttf,otf,TTF,OTF"};
        var path = EditorUtility.OpenFilePanelWithFilters("选择默认字体", "Assets/AssetsPackage/UI/Fonts", filters);
        int index = path.IndexOf("Assets");
        m_fontPath = path.Substring(index, path.Length - index);
    }
}