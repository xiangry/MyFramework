using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class ImageNum : MonoBehaviour
{
    public Image baseImg;
    public int numCount;
}
