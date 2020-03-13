using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
public class ColliderAppend : MonoBehaviour
{
    public delegate void ColliderFunc(Collision other, int TriggerId);
    public delegate void TriggerFunc(Collider other, int TriggerId);

    public int TriggerId; // 触发事件id
    
    private ColliderFunc enterColliderFunc;
    private ColliderFunc stayColliderFunc;
    private ColliderFunc exitColliderFunc;
    
    private TriggerFunc enterTriggerFunc;
    private TriggerFunc stayTriggerFunc;
    private TriggerFunc exitTriggerFunc;

    public void RegisterColliderFunc(ColliderFunc enterFunc, ColliderFunc stayFunc, ColliderFunc exitFunc)
    {
        enterColliderFunc = enterFunc;
        stayColliderFunc = stayFunc;
        exitColliderFunc = exitFunc;
    }

    public void RegisterTriggerFunc(TriggerFunc enterFunc, TriggerFunc stayFunc, TriggerFunc exitFunc)
    {
        enterTriggerFunc = enterFunc;
        stayTriggerFunc = stayFunc;
        exitTriggerFunc = exitFunc;
    }

    #region 碰撞

    private void OnCollisionEnter(Collision other)
    {
        if (enterColliderFunc!=null)
        {
            enterColliderFunc(other, TriggerId);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (stayColliderFunc!=null)
        {
            stayColliderFunc(other, TriggerId);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (exitColliderFunc!=null)
        {
            exitColliderFunc(other, TriggerId);
        }
    }
    #endregion

    

    #region 触发器
    private void OnTriggerEnter(Collider other)
    {
        if (enterTriggerFunc!=null)
        {
            enterTriggerFunc(other, TriggerId);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (stayTriggerFunc!=null)
        {
            stayTriggerFunc(other, TriggerId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (exitTriggerFunc!=null)
        {
            exitTriggerFunc(other, TriggerId);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (enterColliderFunc!=null)
        {
            enterColliderFunc = null;
        }
        if (stayColliderFunc!=null)
        {
            stayColliderFunc = null;
        }
        if (exitColliderFunc!=null)
        {
            exitColliderFunc = null;
        }
        
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

#if UNITY_EDITOR
public static class ColliderAppendExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(ColliderAppend),
        typeof(ColliderAppend.ColliderFunc),
        typeof(ColliderAppend.TriggerFunc),
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(UnityEngine.Collider),
        typeof(UnityEngine.Collision),
    };
}
#endif