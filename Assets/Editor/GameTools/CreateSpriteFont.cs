/*
*R0-V1.0
*Modify Date:2018-11-29
*Modifier:ZoJet
*Modify Reason:根据图片生成包含对应字符的字体文件
*Modify Content:
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.DemiEditor;
using Spine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class CreateSpriteFont : MonoBehaviour  {
    
    [MenuItem("Tools/CreateFont")]
    private static void CreateFont() {

        if (Selection.objects == null || Selection.objects.Length == 0) {
            Debug.Log("No selected object or sharding atlas");
            return;
        }
        
        Object o = Selection.objects[0];
        if (o.GetType() != typeof(Texture2D)) {
            Debug.Log("The selected file is not a picture");
            return;
        }
        
        Texture2D texture2D = o as Texture2D;
        
        string selectionPath = AssetDatabase.GetAssetPath(o);
        TextureImporter textureImporter = TextureImporter.GetAtPath(selectionPath) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.SaveAndReimport();
        
        
        var resPath = Path.GetFileNameWithoutExtension(selectionPath);
        var jsonPath = resPath ;
        var textAsset = Resources.Load<TextAsset>(jsonPath);
        Debug.Log(textAsset);
        var text = textAsset.text;
        var jsonObj = XCModJSON.jsonDecode(textAsset.text) as Hashtable;
        if (! jsonObj.Contains("frames"))
        {
            Debug.LogError("匹配TexturePacker的Txt文件格式错误：没有找到frames");
            return;
        }

        List<SpriteRect> spriteRects = new List<SpriteRect>();
        var frames = jsonObj["frames"] as Hashtable;
        foreach(string key in frames.Keys)
        {
            SpriteRect rect = new SpriteRect();
            rect.alignment = SpriteAlignment.Center;
            rect.name = Path.GetFileNameWithoutExtension(key);
            var frameObj = frames[key] as Hashtable;
            var frame = frameObj["frame"] as Hashtable;
            var x = float.Parse(frame["x"].ToString());
            var y = float.Parse(frame["y"].ToString());
            var w = float.Parse(frame["w"].ToString());
            var h = float.Parse(frame["h"].ToString());
            y = texture2D.height - (y + h);
            rect.rect = new Rect(x, y, w, h);
            spriteRects.Add(rect);
        }
        
       
        UnityEditor.Experimental.U2D.ISpriteEditorDataProvider isp = textureImporter as UnityEditor.Experimental.U2D.ISpriteEditorDataProvider;
        isp.InitSpriteEditorDataProvider();
        textureImporter.SaveAndReimport();
        isp.SetSpriteRects(spriteRects.ToArray());
        isp.Apply();
        textureImporter.SaveAndReimport();




        if (selectionPath.Contains("Resources")) {
            string selectionExt = Path.GetExtension(selectionPath);
            if (selectionExt.Length == 0) {
                return;
            }
            Debug.Log("Selection selectionExt  " + selectionExt);

            string loadPath = selectionPath.Remove(selectionPath.Length - selectionExt.Length);
            string fontPathName = loadPath + ".fontsettings";
            string matPathName = loadPath + ".mat";
            float lineSpace = 0.1f;
            loadPath = Path.GetFileNameWithoutExtension(selectionPath);
            Sprite[] sprites = Resources.LoadAll<Sprite>(loadPath);
            
            if (sprites.Length > 0) {
                Texture2D tex = o as Texture2D;
                Material mat = new Material(Shader.Find("GUI/Text Shader"));
                AssetDatabase.CreateAsset(mat, matPathName);
                mat.SetTexture("_MainTex", tex);
                Font font = new Font();
                font.material = mat;
                AssetDatabase.CreateAsset(font, fontPathName);
                CharacterInfo[] characterInfo = new CharacterInfo[sprites.Length];
 
                for (int i = 0; i < sprites.Length; i++) {
                    if (sprites[i].rect.height > lineSpace) {
                        lineSpace = sprites[i].rect.height;
                    }
                }

                for (int i = 0; i < sprites.Length; i++) {
                    Sprite spr = sprites[i];
                    CharacterInfo info = new CharacterInfo();
                    info.index = (int)spr.name[spr.name.Length - 1];
                    Rect rect = spr.rect;
                    float pivot = spr.pivot.y / rect.height - 0.5f;
                    if (pivot > 0) {
                        pivot = -lineSpace / 2 - spr.pivot.y;
                    } else if (pivot < 0) {
                        pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                    } else {
                        pivot = -lineSpace / 2;
                    }

                    int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                    //设置字符映射到材质上的坐标  
                    info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                    info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                    info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                    info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                    //设置字符顶点的偏移位置和宽高  
                    info.minX = 0;
                    info.minY = -(int)rect.height - offsetY;
                    info.maxX = (int)rect.width;
                    info.maxY = -offsetY;
                    //设置字符的宽度  
                    info.advance = (int)rect.width;
                    characterInfo[i] = info;
                }

                font.characterInfo = characterInfo;
                EditorUtility.SetDirty(font); 
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
  
                Debug.Log("Max Height：" + lineSpace + "  Prefect Height：" + (lineSpace + 2));
            } else {
                Debug.Log("Sprite must be placed in the Resources folder and selected");
            }
        }
    }
}