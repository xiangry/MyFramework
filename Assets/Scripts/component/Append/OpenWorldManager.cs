using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
public class OpenWorldManager : MonoBehaviour
{
    public delegate void TriggerFunc(Collider other, int TriggerId, int TriggerType);
    
    public void OpenWorldRegist(TriggerFunc enter, TriggerFunc stayTrigger, TriggerFunc exitTrigger)
    {
        OpenWorldTrigger[] WorldTriggers = GetComponentsInChildren<OpenWorldTrigger>();
        for (int i = 0; i < WorldTriggers.Length; i++)
        {
            WorldTriggers[i].RegisterWorldTriggerFunc(enter, stayTrigger, exitTrigger);
        }
    }
}

#if UNITY_EDITOR
public static class OpenWorldManagerExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(OpenWorldManager.TriggerFunc),
    };
}
#endif