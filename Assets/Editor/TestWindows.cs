using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TestWindows : EditorWindow
{
    [MenuItem("Window/测试用例")]
    public static void OpenSetFontWindow()
    {
        EditorWindow window = GetWindow(typeof (TestWindows));
        window.minSize = new Vector2(100, 80);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("选择默认字体");
        EditorGUILayout.Space();
    }
}