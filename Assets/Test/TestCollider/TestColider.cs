using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;

public class TestColider : MonoBehaviour
{
    private Vector2 swipeStart;
    private ObjInfo curSelectObj = null;
    private Color curSelectColor;

    List<ObjInfo> aliveObjs = new List<ObjInfo>();

    void Start()
    {
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;

        EasyTouch.On_DragStart += EasyTouch_On_SwipeBegin;
        EasyTouch.On_Drag += EasyTouch_On_Swipe;
        EasyTouch.On_DragEnd += EasyTouch_On_SwipeEnd;
        
        EasyTouch.On_SwipeStart += EasyTouch_On_SwipeBegin;
        EasyTouch.On_Swipe += EasyTouch_On_Swipe;
        EasyTouch.On_SwipeEnd += EasyTouch_On_SwipeEnd;

        aliveObjs.Add(GameObject.Find("yellow").AddComponent<ObjInfo>());
        aliveObjs.Add(GameObject.Find("green").AddComponent<ObjInfo>());
        aliveObjs.Add(GameObject.Find("blue").AddComponent<ObjInfo>());
    }

    void EasyTouch_On_TouchStart(Gesture gesture)
    {
        GameObject obj = CheckTouchObj(gesture);
        if (obj != null)
        {
            if (curSelectObj == null || obj != curSelectObj.gameObject)
            {
                UnSelectObj();
                SelectObj(obj);
            }
        }
    }

    void EasyTouch_On_SwipeBegin(Gesture gesture)
    {
        swipeStart = gesture.position;
    }

    void EasyTouch_On_Swipe(Gesture gesture)
    {
        Vector2 offset = gesture.position - swipeStart;
        if (curSelectObj)
        {
            curSelectObj.MoveOffset(offset);
        }
    }

    void EasyTouch_On_SwipeEnd(Gesture gesture)
    {
        if (curSelectObj)
        {
            curSelectObj.ApplayOffset();
        }
    }

    void SelectObj(GameObject obj)
    {
        if(GetAliveObj(obj) != null)
        {
            curSelectObj = GetAliveObj(obj);
            curSelectObj.Select();
        }
    }

    void UnSelectObj()
    {
        if (curSelectObj != null)
        {
            curSelectObj.UnSelect();
            curSelectObj = null;
        }
    }

    GameObject CheckTouchObj(Gesture gesture)
    {
        Ray ray = Camera.main.ScreenPointToRay(gesture.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    ObjInfo GetAliveObj(GameObject obj)
    {
        foreach (var aliveObj in aliveObjs)
        {
            if (aliveObj.gameObject == obj)
            {
                return aliveObj;
            }
        }
        return null;
    }
}
