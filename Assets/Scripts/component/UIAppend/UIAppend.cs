using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace component.UIAppend
{
    [LuaCallCSharp()]
    public static class UIAppend 
    {

        public static void SetColor(this Image image, float r, float g, float b, float a)
        {
            Color myColor = new Color(r, g, b, a);
            image.color = myColor;
        }


        public static void SetColor(this Text text, float r, float g, float b, float a)
        {
            Color mycolor = new Color(r,g,b,a);
            text.color = mycolor;
        }

    }
}
