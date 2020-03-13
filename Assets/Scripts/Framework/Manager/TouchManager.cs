﻿//EasyTouch的触摸管理 使用方法 在lua侧场景中通过	
// self._layer = UtilityLuaCallCS.CreateTouchLayer(ConstTouchLayer.Explorer, 需要注册的模块l自身lua table) 注册 
// UtilityLuaCallCS.AddTouchLayer(self._layer) 添加监听
// lua中 直接 写OnTap OnTouchStart OnTouchEnd方法


using HedgehogTeam.EasyTouch;
using XLua;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

    
#if UNITY_EDITOR
public static class TouchManagerExporter
{
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
        typeof(Sword.EventCall),
    };
}
#endif


namespace Sword
{
    
    [CSharpCallLua]
    public delegate bool EventCallLua(LuaTable targetLua, Gesture gt);
    
    [LuaCallCSharp()]
    public delegate bool EventCall(Gesture gt);

    [LuaCallCSharp()]
    public class LayerInfo
    {
        private Dictionary<int, EventCall> _calls = new Dictionary<int, EventCall>();
        private int _layer = TouchLayerIndex.Attacker;
        public int Layer
        {
            get
            {
                return _layer;
            }
        }
        public LayerInfo(int layer)
        {
            _layer = layer;
        }
        public virtual EventCall this[int index]
        {
            get
            {
                if (_calls.ContainsKey(index))
                {
                    return _calls[index];
                }
                return null;
            }
            set { _calls[index] = value; }
        }
    }
    
    [LuaCallCSharp()]
    public class TouchManager : Singleton<TouchManager>
    {
        public SortedList<int, LayerInfo> _allLayers = new SortedList<int, LayerInfo>();
        private bool mIsTouch = true;

        private bool mIsInit = false;
        //private NotifyDispatcher _touchNotify = new NotifyDispatcher(NotifyDispatcher.Type.Touch);
        EasyTouch mET;
        GameObject go;
        
        public TouchManager()
        {

        }

        public override void Init()
        {
            if (!mIsInit)
            {
                EasyTouch.On_SimpleTap += OnTap;
                EasyTouch.On_DoubleTap += OnDoubleTap;
                EasyTouch.On_TouchStart += OnTouchStart;
                EasyTouch.On_TouchUp += OnTouchEnd;

                EasyTouch.On_SwipeStart += OnSwipeBegin;
                EasyTouch.On_Swipe += OnSwipe;
                EasyTouch.On_SwipeEnd += OnSwipeEnd;

                EasyTouch.On_DragStart += OnDragBegin;
                EasyTouch.On_Drag += OnDrag;
                EasyTouch.On_DragEnd += OnDragEnd;

                EasyTouch.On_TouchStart2Fingers += OnPinchBegin;
                EasyTouch.On_PinchIn += OnPinchIn;
                EasyTouch.On_PinchOut += OnPinchOut;
                EasyTouch.On_TouchUp2Fingers += OnPinchEnd;

                EasyTouch.On_UIElementTouchUp += OnUIElimentTouchUp;

                go = GameObject.Find("Scene/Event");
                mET = go.GetComponent<EasyTouch>();

                mIsInit = true;
            }

        }

        public override void Dispose()
        {
            Clear();
//            throw new NotImplementedException();
        }

        public void SetEasyTouch(bool enable)
        {
            mIsTouch = enable;
        }

        //h ack: has not clear yet
        public void Clear()
        {
            EasyTouch.On_SimpleTap -= OnTap;
            EasyTouch.On_DoubleTap -= OnDoubleTap;
            EasyTouch.On_TouchStart -= OnTouchStart;
            EasyTouch.On_TouchUp -= OnTouchEnd;

            EasyTouch.On_SwipeStart -= OnSwipeBegin;
            EasyTouch.On_Swipe -= OnSwipe;
            EasyTouch.On_SwipeEnd -= OnSwipeEnd;

            EasyTouch.On_DragStart -= OnDragBegin;
            EasyTouch.On_Drag -= OnDrag;
            EasyTouch.On_DragEnd -= OnDragEnd;

            EasyTouch.On_TouchStart2Fingers -= OnPinchBegin;
            EasyTouch.On_PinchIn -= OnPinchIn;
            EasyTouch.On_PinchOut -= OnPinchOut;
            EasyTouch.On_TouchUp2Fingers -= OnPinchEnd;
            mIsInit = false;
        }

        #region EasyTouch Events
        public void OnTap(Gesture gt)
        {
            if (UtilClick.IsVaildTouch())
            {
                DateTime now = DateTime.Now;
                UtilClick.last = now;
                Do(TouchEventType.Tap, gt);
            }
        }

        public void OnDoubleTap(Gesture gt)
        {
            OnTap(gt);
        }
        public void GuideEvent(int type, Gesture gt)
        {
            Do(type, gt);
        }
        public void OnTouchStart(Gesture gt)
        {
            
            Do(TouchEventType.TouchStart, gt);
        }
        public void OnTouchEnd(Gesture gt)
        {
            Do(TouchEventType.TouchEnd, gt);
        }
        public void OnSwipeBegin(Gesture gt)
        {
            Do(TouchEventType.SwipeBegin, gt);
        }
        public void OnSwipe(Gesture gt)
        {
            Do(TouchEventType.Swipe, gt);
        }
        public void OnSwipeEnd(Gesture gt)
        {
            Do(TouchEventType.SwipeEnd, gt);
        }
        public void OnDragBegin(Gesture gt)
        {
            Do(TouchEventType.DragBegin, gt);
        }
        public void OnDrag(Gesture gt)
        {
            Do(TouchEventType.Drag, gt);

        }
        public void OnDragEnd(Gesture gt)
        {
            Do(TouchEventType.DragEnd, gt);
        }
        public void OnPinchBegin(Gesture gt)
        {
            Do(TouchEventType.PinchBegin, gt);
        }
        public void OnPinchIn(Gesture gt)
        {
            Do(TouchEventType.PinchIn, gt);
        }
        public void OnPinchOut(Gesture gt)
        {
            Do(TouchEventType.PinchOut, gt);
        }
        public void OnPinchEnd(Gesture gt)
        {
            Do(TouchEventType.PinchEnd, gt);
        }
        public void OnUIElimentTouchUp(Gesture gt)
        {
            Do(TouchEventType.TouchUI, gt);
        }
        #endregion

        private void Do(int type, Gesture gt)
        {
            try
            {

                if (mIsTouch == false)
                {
                    return;
                }

                EventCall call = null;
                var iter = _allLayers.GetEnumerator();
                while (iter.MoveNext())
                {
                    var item = iter.Current;
                    call = item.Value[type];
                    if (call != null)
                    {
                        if (call(gt))
                        {
                            //todo 修改派发 
//                            Facade.TouchNotify.Dispatch((item.Value.Layer), type);
                            break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                    Logger.LogError(ex.ToString()); 
            }

        }
        
        [LuaCallCSharp()]
        public void AddEvent(int layer, int type, EventCall call)
        {
            if (!_allLayers.ContainsKey(layer))
            {
                _allLayers.Add(layer, new LayerInfo(layer));
            }
            _allLayers[layer][type] = call;
        }
        
        
        [LuaCallCSharp()]
        public void AddEvent1(int layer, int type)
        {
            Debug.Log("====================AddEvent1====================");
        }
        
        [LuaCallCSharp()]
        public void RemoveEvent(int layer, int type, EventCall call)
        {
            _allLayers[layer][type] = null;
        }

        public LayerInfo this[int index]
        {
            get { return _allLayers[index]; }
        }
        public void AddLayer(LayerInfo layer)
        {
            if (!_allLayers.ContainsKey(layer.Layer))
            {
                _allLayers.Add(layer.Layer, layer);
            }
        }
        public void RemoveLayer(LayerInfo layer)
        {
            _allLayers.Remove(layer.Layer);
        }
        public void RemoveLayer(int key)
        {
            _allLayers.Remove(key);
        }

        public static bool IsPointerOverUIObject()
        {
#if UNITY_EDITOR
            return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IPHONE
			return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#else
			return EventSystem.current.IsPointerOverGameObject();
#endif
        }
    }
}