using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public class KVImageHelper : AssetPostprocessor
{
    static KVImageHelper()
    {
        Initialize();
    }

    static void Initialize()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= KVImageHelper.HandleDragAndDrop;
        EditorApplication.hierarchyWindowItemOnGUI += KVImageHelper.HandleDragAndDrop;
    }

    private static void HandleDragAndDrop(int instanceid, Rect selectionrect)
    {
        var current = UnityEngine.Event.current;
        var eventType = current.type;
        bool isDraggingEvent = eventType == EventType.DragUpdated;
        bool isDragExited = eventType == EventType.DragExited;
        if (isDraggingEvent || isDragExited)
        {
            var mouseOverWindow = EditorWindow.mouseOverWindow;
            if (mouseOverWindow != null)
            {
                var references = DragAndDrop.objectReferences;
                if (references.Length == 1)
                {
                    var texture = references[0] as Texture2D;
                    if (texture != null)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        if ("UnityEditor.SceneHierarchyWindow".Equals(mouseOverWindow.GetType().ToString(),
                            StringComparison.Ordinal))
                        { 
                            if (isDraggingEvent)
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                                current.Use();
                            }
                            else if (isDragExited)
                            {
                                if (selectionrect.Contains(current.mousePosition))
                                {
                                    GameObject gameObject = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
                                    ShowInstantiateContextMenu(texture, DragAndDrop.paths[0], gameObject);
                                    DragAndDrop.AcceptDrag();
                                    current.Use();
                                }
                            }
                        }
                    }
                }
            }
        }

//        if (current.type != EventType.Layout
//            && current.type != EventType.MouseUp
//            && current.type != EventType.MouseDown
//            && current.type != EventType.Repaint)
//        {
//            if (selectionrect.Contains(current.mousePosition))
//            {
//                GameObject gameObject = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
//                string name = gameObject != null ? gameObject.name : "null";
//                Logger.LogError($"gameObject ==== {name} {instanceid} {current.type} {selectionrect}");
//            }
//        }
    }

    public static void ShowInstantiateContextMenu (Texture2D texture2D, string path, GameObject parent) {
        if (parent.GetComponent<Canvas>() == null && parent.GetComponentInParent<Canvas>() == null)
        {
            return;
        }

        Selection.activeTransform = parent.transform;
        TextureImporter textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter.textureType == TextureImporterType.Default)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            InstantiateRawImage(texture2D, name);
        }
        else if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            InstantiateImage(sprite, name);

            SpriteRenderer[] sprs = parent.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spr in sprs)
            {
                if (spr.name == name)
                {
                    GameObject.DestroyImmediate(spr.gameObject);
                }
            }
        }
    }

    static RawImage InstantiateRawImage(Texture2D texture2D, string name)
    {
        RawImage img = UIRaycastTarget.CreatRawImage();
        if (img)
        {
            img.texture = texture2D;
            img.name = name;
            img.SetNativeSize();
            return img;
        }

        return null;
    }
    
    static Image InstantiateImage(Sprite sprite, string name)
    {        
        Image img = UIRaycastTarget.CreatImage();
        if (img)
        {
            img.sprite = sprite;
            img.name = name;
            img.SetNativeSize();
            return img;
        }
        return null;
    }
}
