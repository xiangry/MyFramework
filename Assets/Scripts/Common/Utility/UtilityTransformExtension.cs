using System.Net.Security;
using UnityEngine;
using XLua;

namespace Common.Utility
{
    [LuaCallCSharp()]
    public static class UtilityTransformExtension
    {
        public static Transform  FindRec(this Transform trans, string goName)
        {
            if (trans.name == goName) {
                return trans;
            }
            Transform child = trans.Find(goName);
            if (child != null)
            {
                return child;
            }

            Transform go = null;
            for (int i = 0; i < trans.childCount; i++)
            {
                child = trans.GetChild(i);
                go = FindRec(child, goName);
                if (go != null)
                {
                    return go;
                }
            }
            return null;
        }

        public static void SetLocalScale(this Transform trans, float x, float y, float z)
        {
            Vector3  scale = new Vector3(x, y, z);
            trans.localScale = scale;
        }

        public static void SetLocalPosition(this Transform trans, float x, float y, float z)
        {
            Vector3 position = new Vector3(x, y, z);
            trans.localPosition = position;
        }


        public static void SetRectPos(this Transform trans, float x, float y, float z)
        {
            var rtrans = trans as RectTransform;
            if (rtrans != null)
            {
                rtrans.anchoredPosition3D = new Vector3(x, y, z);
            }
        }

        public static void SetSizeDelta(this RectTransform ret, float x, float y)
        {
            ret.sizeDelta = new Vector2(x, y);
        }
    }
}