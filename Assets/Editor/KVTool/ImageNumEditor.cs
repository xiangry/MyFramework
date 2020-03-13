using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ImageNum))]
public class ImageNumEditor : Editor
{
    [MenuItem("GameObject/UI/User/ImageNum")]
    static void CreatRawImage()
    {
        if(Selection.activeTransform)
        {
            if(Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("ImageNum",typeof(RectTransform));
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0,0,0);
                go.transform.localScale = Vector3.one;
                go.layer = LayerMask.NameToLayer("UI");
                
                Image baseImg = UIRaycastTarget.CreatImage();
                baseImg.transform.parent = go.transform;
                baseImg.transform.name = "defalut";

                ImageNum imageNum = go.AddComponent<ImageNum>();
                imageNum.baseImg = baseImg;
                imageNum.numCount = 1;
                
                GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
                grid.childAlignment = TextAnchor.MiddleCenter;

            }
        }
    }

    public override void OnInspectorGUI()
    {
//        base.OnInspectorGUI();

        ImageNum imageNum = target as ImageNum;

        EditorGUILayout.Space();
        int lastCount = imageNum.numCount;
        imageNum.numCount = EditorGUILayout.IntField("默认数字位数", imageNum.numCount);
        EditorGUILayout.Space();
        imageNum.baseImg = EditorGUILayout.ObjectField("指定数字精灵", imageNum.baseImg, typeof(Image)) as Image;
        EditorGUILayout.Space();

        if (imageNum.numCount <= 0)
        {
            EditorGUILayout.HelpBox("默认数量必须大于等于1", MessageType.Error);
            return;
        }

        if (imageNum.numCount >= 15)
        {
            imageNum.numCount = 15;
            EditorGUILayout.HelpBox("默认显示数字为15位数", MessageType.Error);
            return;
        }


        if (imageNum.baseImg == null)
        {
            EditorGUILayout.HelpBox("必须指定一个默认显示数字精灵", MessageType.Error);
            return;
        }

        if (imageNum.baseImg.transform.parent != imageNum.transform)
        {
            EditorGUILayout.HelpBox("指定的精灵必须挂载到本节点", MessageType.Error);
            return;
        }

        Image baseImage = imageNum.baseImg;
        baseImage.gameObject.SetActive(false);
        
        var width = baseImage.rectTransform.rect.width;
        var height = baseImage.rectTransform.rect.height;

        GridLayoutGroup grid = imageNum.gameObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(width, height);
        grid.rectTransform().sizeDelta = new Vector2(width * imageNum.numCount, height);
        
        if (lastCount != imageNum.numCount)
        {
            Image[] numImgs = imageNum.transform.GetComponentsInChildren<Image>();

            foreach (var num in numImgs)
            {
                if (num == baseImage)
                {
                    continue;
                }

                GameObject.DestroyImmediate(num.gameObject);
            }

            for (int i = 1; i <= imageNum.numCount; i++)
            {
                GameObject obj = Instantiate(baseImage.gameObject, imageNum.transform.parent);
                obj.transform.parent = imageNum.transform;
                obj.name = i.ToString();
                obj.SetActive(true);
            }
        }
    }
}
