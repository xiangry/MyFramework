using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XLua;

[LuaCallCSharp()]
[RequireComponent(typeof(EventTrigger))]
public class EventTriggerAppend : MonoBehaviour
{
    public void AddEventTrigger(EventTriggerType triggerType, UnityAction<BaseEventData> callback)
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = triggerType;
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }
}

#if UNITY_EDITOR
public static class EventTriggerAppendExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(BaseEventData),
        typeof(UnityAction<BaseEventData>),
    };
}
#endif