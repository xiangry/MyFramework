using System.Configuration;
using UnityEngine.UI;
using UnityEngine;
using XLua;

namespace Common.Utility
{
    [LuaCallCSharp()]
    public static class UtilityUI
    {
        public static void SetValue(this Toggle toggle, bool value)
        {
            toggle.isOn = value;
        }
    }
}