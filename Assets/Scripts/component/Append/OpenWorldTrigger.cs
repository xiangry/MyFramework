using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
public class OpenWorldTrigger : MonoBehaviour
{
    public int triggerId; // 触发事件id
    public int triggerType; // 触发事件id
    public string triggerMessage; // Trigger说明

    private OpenWorldManager.TriggerFunc enterTriggerFunc;
    private OpenWorldManager.TriggerFunc stayTriggerFunc;
    private OpenWorldManager.TriggerFunc exitTriggerFunc;

    public void RegisterWorldTriggerFunc(OpenWorldManager.TriggerFunc enterFunc, 
        OpenWorldManager.TriggerFunc stayFunc, OpenWorldManager.TriggerFunc exitFunc)
    {
        enterTriggerFunc = enterFunc;
        stayTriggerFunc = stayFunc;
        exitTriggerFunc = exitFunc;
    }

    #region 触发器
    private void OnTriggerEnter(Collider other)
    {
        if (enterTriggerFunc!=null)
        {
            enterTriggerFunc(other, triggerId, triggerType);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (stayTriggerFunc!=null)
        {
            stayTriggerFunc(other, triggerId, triggerType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (exitTriggerFunc!=null)
        {
            exitTriggerFunc(other, triggerId, triggerType);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (enterTriggerFunc!=null)
        {
            enterTriggerFunc = null;
        }
        if (stayTriggerFunc!=null)
        {
            stayTriggerFunc = null;
        }
        if (exitTriggerFunc!=null)
        {
            exitTriggerFunc = null;
        }
    }
}