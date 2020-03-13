using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIRaycastTarget
{
    [MenuItem("GameObject/UI/Image")]
    public static Image CreatImage()
    {
        if(Selection.activeTransform)
        {
            if(Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Image",typeof(Image));
                go.transform.SetParent(Selection.activeTransform);
                go.GetComponent<Image>().raycastTarget = false;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0,0,0);
                go.transform.localScale = Vector3.one;
                go.layer = LayerMask.NameToLayer("UI");
                Selection.activeGameObject = go;
                return (go.GetComponent<Image>());
            }
        }

        return null;
    }
    
    [MenuItem("GameObject/UI/Raw Image")]
    static public RawImage CreatRawImage()
    {
        if(Selection.activeTransform)
        {
            if(Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("RawImage",typeof(RawImage));
                go.transform.SetParent(Selection.activeTransform);
                go.GetComponent<RawImage>().raycastTarget = false;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0,0,0);
                go.transform.localScale = Vector3.one;
                go.layer = LayerMask.NameToLayer("UI");                
                Selection.activeGameObject = go;
                return go.GetComponent<RawImage>();
            }
        }

        return null;
    }
    
    [MenuItem("GameObject/UI/Text")]
    static void CreatText()
    {
        if(Selection.activeTransform)
        {
            if(Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Text",typeof(Text));
                go.transform.SetParent(Selection.activeTransform);
                
                Text text = go.GetComponent<Text>();
                text.raycastTarget = false;
                text.text = "New Text";
                text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                Rect rect = text.rectTransform.rect;
                text.rectTransform.sizeDelta = new Vector2(100, 30);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0,0,0);
                go.transform.localScale = Vector3.one;
                go.layer = LayerMask.NameToLayer("UI");

                Font font = ToolCacheManager.GetFont();
                text.font = font;

                Selection.activeGameObject = go;
            }
        }
    }
}
