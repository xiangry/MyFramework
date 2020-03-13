using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class CoroutineAppend : MonoBehaviour
{

    [LuaCallCSharp()]
    public void DoFuncWaitEndFrame(Action action)
    {
        StartCoroutine(ExcuteWaitEndFrameFunc(action));
    }

    IEnumerator ExcuteWaitEndFrameFunc(Action action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
        action = null;
    }
}
