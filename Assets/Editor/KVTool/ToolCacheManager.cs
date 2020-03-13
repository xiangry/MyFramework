using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ToolCacheManager
{
    static public string ToolKitSettingPath = "Assets/toolkit/setting";
    static public string DefaultFontAsset = "DefaultUIFont.asset";
    static public string DefaultFontPath = "Assets/AssetsPackage/UI/Fonts/ui_font.OTF";
    
    public static void SaveFont(string fontPath)
    {
        if (!Directory.Exists(ToolKitSettingPath))
        {
            Directory.CreateDirectory(ToolKitSettingPath);
        }
        FontInfo data = ScriptableObject.CreateInstance<FontInfo>();
        data.defaultFont = fontPath;
        AssetDatabase.CreateAsset(data, Path.Combine(ToolKitSettingPath, DefaultFontAsset));
    }

    public static Font GetFont()
    {
        FontInfo data = AssetDatabase.LoadAssetAtPath<FontInfo>(Path.Combine(ToolKitSettingPath, DefaultFontAsset));
        if (data && data.defaultFont != null)
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(data.defaultFont);
            if (font != null)
            {
                return font;
            }
        }
        
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(DefaultFontPath);
            return font;
        }
    }
    
    public static string GetFontPath()
    {
        FontInfo data = AssetDatabase.LoadAssetAtPath<FontInfo>(Path.Combine(ToolKitSettingPath, DefaultFontAsset));
        if (data && data.defaultFont != null)
        {
            return data.defaultFont;
        }

        return DefaultFontPath;
    }
}

[System.Serializable]
public class FontInfo : ScriptableObject
{
    [SerializeField]
    public string defaultFont;
}