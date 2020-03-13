using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnityUIEvent
{
    //[InitializeOnLoadMethod]
    //private static void Init()
    //{
    //    Action OnEvent = delegate
    //    {
    //        ChangeDefaultFont();
    //    };

    //    EditorApplication.hierarchyWindowChanged = delegate()
    //    {
    //        OnEvent();
    //    };
    //}

    //private static void ChangeDefaultFont()
    //{
    //    if (Application.isPlaying)
    //    {
    //        return;
    //    }
    //    if (Selection.activeGameObject != null)
    //    {
    //        Text[] texts = Selection.activeGameObject.GetComponentsInChildren<Text>();
    //        if (texts != null && texts.Length > 0)
    //        {
    //            Font font = ToolCacheManager.GetFont();
    //            if (font == null)
    //            {
    //                SetDefaultFont.OpenWindow(() =>
    //                {
    //                    font = ToolCacheManager.GetFont();
    //                    if (font)
    //                    {
    //                        ChangeAllTextDefaultFont(texts, font);
    //                    }
    //                    else
    //                    {
    //                        Logger.LogError("没有设置默认字体或设置失败");
    //                    }
    //                });
    //            }
    //            else
    //            {
    //                ChangeAllTextDefaultFont(texts, font);
    //            }
    //        }
    //    }
    //}

    //public static void ChangeAllTextDefaultFont(Text[] texts, Font font)
    //{
    //    foreach (var text in texts)
    //    {
    //        if(text.font.name == "Arial")
    //        {
    //            //text.font = font;
    //        }
    //    }
    //}
}