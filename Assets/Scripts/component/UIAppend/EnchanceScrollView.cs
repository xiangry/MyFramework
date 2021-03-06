﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;
using DG;
using Sword;


namespace component.UIAppend
{
    [LuaCallCSharp()]
    public class EnchanceScrollView : MonoBehaviour,IBeginDragHandler,IEndDragHandler
    
    {
        [CSharpCallLua]
        public delegate void EScrollRefresh(int index, GameObject go);
        [CSharpCallLua]
        public delegate void EScrollOver(int index);  //移动结束的回调（并不是结束，而是确定会移动到那一个index）

        private EScrollRefresh refresh;
        private EScrollOver overcall;
        
        public ScrollRect Scroll;
        public GameObject Target;
        public GridLayoutGroup Grid;

        public int Num;

        public float MoveSpeed;

        private bool _isInit = false;
        private Dictionary<GameObject, int> datasAndIndex = new Dictionary<GameObject, int>();
        private List<GameObject> needDispose = new List<GameObject>();

        public float MoveDistance;
        public bool MoveFlag;

        private void Start()
        {
            if (Num == -1) Num = 10;
            if (MoveSpeed == 0) MoveSpeed = 5;
            MoveDistance = 0;

            Grid.rectTransform().anchorMin = new Vector2(0, 1);
            Grid.rectTransform().anchorMax = new Vector2(0, 1);

            Grid.enabled = false;
            if (Target == null)
            {
                throw new Exception("还没有设置需要初始化的物品");
            }

            Target.SetActive(false);
            if (Grid == null)
            {
                throw new Exception("还没有设置好LayOut组件！！！");
            }

//            StartCoroutine(Init());

            Scroll.onValueChanged.AddListener(OnListenMove);
        }

        public void RemoveListener()
        {
            if (refresh != null)
            {
                refresh = null;
            }

            if (overcall != null)
            {
                overcall = null;
            }
        }

     
        public void OnDestroy()
        {
//            for (int i = 0; i < temp.Count; i++)
//            {
//                temp[i].DestroySelf();
//            }

            RemoveListener();
        }

        #region public

        //===========================================================================================
        public void SetNum(int num)
        {
            Num = num;
            StartCoroutine(Init());
        }

        public void AddRefresh(EScrollRefresh func)
        {
            if (refresh != null)
            {
                refresh += func;
            }
            else
            {
                refresh = func;
            }
        }

        public void AddOverCall(EScrollOver func)
        {
            if (overcall != null)
            {
                overcall += func;
            }
            else
            {
                overcall = func;
            }
        }

        private Coroutine m_Coroutine;

        public void MoveToIndex(int index, float delay = 0.5f)
        {
            if (index < 0 || index > Num - 1)
                throw new Exception("需要定位的位置错误：" + index);

            index = Math.Min(index, Num - GetShowCount() + 2);
            index = Math.Max(0, index);

            var rect = Grid.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;

            Vector2 V2Pos;
            if (Scroll.horizontal)
            {
                V2Pos = new Vector2(-GetPos(index), rect.anchoredPosition.y);
            }
            else
            {
                V2Pos = new Vector2(rect.anchoredPosition.x, -GetPos(index));
            }
            
            m_Coroutine = StartCoroutine(TweenMoveToPos(pos, V2Pos, delay));
            overcall?.Invoke(index); //调用结束回调
        }

        public void SetToIndex(int index)
        {
            if (index < 0 || index > Num - 1)
                throw new Exception("需要定位的位置错误：" + index);

            index = Math.Min(index, Num - GetShowCount() + 2);
            index = Math.Max(0, index);

            var rect = Grid.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;

            Vector2 V2Pos;
            if (Scroll.horizontal)
            {
                V2Pos = new Vector2(-GetPos(index), rect.anchoredPosition.y);
            }
            else
            {
                V2Pos = new Vector2(rect.anchoredPosition.x, -GetPos(index));
            }

            if (Scroll.horizontal)
            {
                Grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(V2Pos.x-1, V2Pos.y);
            }
            else
            {
                Grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(V2Pos.x, V2Pos.y-1);
            }

           
            m_Coroutine = StartCoroutine(TweenMoveToPos(pos, V2Pos, 0.2f));
        }

        #endregion
//================================================================================================

        #region private

        private void SetItemRecord(int index, RectTransform go)
        {    
            if (index > Num) return;
            datasAndIndex[go.gameObject] = index;
            
            //设置位置
            go.pivot = new Vector2(0, 1);
            if (Scroll.horizontal && !Scroll.vertical) //横向
            {
                go.anchoredPosition3D = new Vector3(GetPos(index), 0, 0);
            }
            else if (!Scroll.horizontal && Scroll.vertical) //纵向
            {
                go.anchoredPosition3D = new Vector3(0, GetPos(index), 0);
            }

            go.gameObject.name = "item" + index;
            refresh?.Invoke(index, go.gameObject);
        }

        //获取需要展示的数量
        private List<GameObject> temp = new List<GameObject>();

        private IEnumerator Init()
        {
            _isInit = true;
            //0.设置物体的框和高
            var _rect = Grid.rectTransform();
            if (Scroll.horizontal)
            {
                _rect.sizeDelta = new Vector2((Grid.cellSize.x + Grid.spacing.x) * (Num), _rect.sizeDelta.y);
            }
            else
            {
                _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, (Grid.cellSize.y + Grid.spacing.y) * Num);
            }

            _rect.localPosition = Vector3.zero;

            //1.初始化物体
            if ( Num > 0)
            {
                Grid.gameObject.SetActive(true);
                if (datasAndIndex.Count > 0)
                {
                    foreach (var go in datasAndIndex.Keys)
                    {
                        
                        go.name = "recycle";
                        temp.Add(go);
                    }
                    datasAndIndex.Clear();
                }

                for (int i = 0; i < (Num < GetShowCount() ? Num : GetShowCount()); i++)
                {
                    GameObject obj;

                    if (temp.Count > 0)
                    {
                        obj = temp[0];
                        temp.RemoveAt(0);
                    }
                    else
                    {
                        obj = Instantiate(Target);
                        needDispose.Add(obj);
                    }

                    obj.transform.SetParent(Grid.transform);
                    obj.SetActive(true);
                    obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    var rect = obj.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(Grid.cellSize.x, Grid.cellSize.y);
                    rect.anchorMin = new Vector2(0,1);
                    rect.anchorMax = new Vector2(0,1);
                    SetItemRecord(i, obj.GetComponent<RectTransform>());
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].gameObject.SetActive(false);
                }
            }
            else
            {
                Grid.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.1f);
            _isInit = false;
        }

        private int GetShowCount()
        {
            int showCount = 0;
            //1.首先判断是横向还是纵向：
            if (Scroll.horizontal && !Scroll.vertical) //横向
            {
                float width = Scroll.gameObject.GetComponent<RectTransform>().sizeDelta.x;
                float item_width = Grid.cellSize.x + Grid.spacing.x;
                showCount = Mathf.CeilToInt(width / item_width);
            }
            else if (!Scroll.horizontal && Scroll.vertical) //纵向
            {
                float height = Scroll.gameObject.GetComponent<RectTransform>().sizeDelta.y;
                float item_height = Grid.cellSize.y + Grid.spacing.y;
                showCount = Mathf.CeilToInt(height / item_height);
            }
            else
            {
                throw new Exception("滑动列表滑动方向设置错误！！！");
            }

            return showCount + 1;
        }

        //拖动监听方法
        private void OnListenMove(Vector2 vector)
        {
            if (_isInit == true) return;

            if (Num < GetShowCount()) return; //展示的数量超过需要的数量，不可滑动
            int indexNow;
            float tempDistance;
            float itemLength;
            if (Scroll.horizontal)
            {
                indexNow = GetIndex(Grid.GetComponent<RectTransform>().anchoredPosition3D.x); //返回当前的是第几个物体
            }
            else
            {
                indexNow = GetIndex(Grid.GetComponent<RectTransform>().anchoredPosition3D.y); //返回当前的是第几个物体
            }

            foreach (var go in datasAndIndex.Keys)
            {
                if (datasAndIndex[go] >= indexNow && datasAndIndex[go] < indexNow + GetShowCount())
                {
                    //没有超出范围，
                    continue;
                }
                else
                {
                    go.name = "recycle";
                    //超出范围，收回到对象池内
                    needDispose.Add(go);
                }
            }

            foreach (var go in needDispose)
            {
                datasAndIndex.Remove(go);
                go.gameObject.SetActive(false);
            }
            

            if (Scroll.horizontal)
            {
                itemLength = Grid.cellSize.x + Grid.spacing.x;
                tempDistance = Grid.GetComponent<RectTransform>().anchoredPosition3D.x; //返回当前物体的x坐标
            }
            else
            {
                itemLength = Grid.cellSize.y + Grid.spacing.y;
                tempDistance = Grid.GetComponent<RectTransform>().anchoredPosition3D.y; //返回当前物体的y坐标
            }

            if (Math.Abs(Math.Abs(tempDistance) - Math.Abs(MoveDistance)) < 0.1)
            {
                Scroll.StopMovement();
                MoveFlag = true;
            }
            
            if ( MoveFlag == true && isdrag == true)
            {
                double indexNum = Math.Ceiling(Math.Abs(tempDistance) / itemLength);
                double len = Math.Abs(tempDistance) % itemLength;
                if ((Num - indexNum) > GetShowCount()-2)
                {
                    if (len * 2 < itemLength)
                    {
                        if (indexNum-1 < 0 || indexNum -1 > Num - 1)
                        {
                            return;
                        }
                    
                        MoveToIndex((int)indexNum-1, 0.3f);
                    }
                    else
                    {
                        if (indexNum < 0 || indexNum> Num - 1)
                        {
                            return;
                        }
                        MoveToIndex((int)indexNum, 0.3f);
                    }
                }
                
            }
            else
            {
                MoveDistance = tempDistance;
            }

            for (int i = indexNow; i < indexNow + GetShowCount(); i++)
            {
                if (datasAndIndex.ContainsValue(i))
                {
                    continue;
                }
                else
                {
                    if (i < Num)
                    {
                        RectTransform item;
                        if (needDispose.Count > 0)
                        {
                            item = needDispose[0].GetComponent<RectTransform>();
                            needDispose.RemoveAt(0);
                        }
                        else
                        {
                            item = Instantiate(Target).GetComponent<RectTransform>();
                        }

                        item.transform.SetParent(Grid.transform);
                        item.gameObject.SetActive(true);
                        item.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        item.sizeDelta = new Vector2(Grid.cellSize.x, Grid.cellSize.y);
                        SetItemRecord(i, item);
                    }
                }
            }
        }


        //根据当前位置信息获取对应的Index
        private int GetIndex(float curps)
        {
            int index = 0;
            float size = 0;
            for (int i = 0; i < Num; i++)
            {
                if (Scroll.horizontal && !Scroll.vertical) //横向
                {
                    size -= (Grid.cellSize.x + Grid.spacing.x);
                    if (size < curps)
                    {
                        index = i;
                        break;
                    }
                }
                else if (!Scroll.horizontal && Scroll.vertical) //纵向
                {
                    size += Grid.cellSize.y + Grid.spacing.y;

                    if (size > curps)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

        private float GetPos(int index)
        {
            float size = 0;
            for (int i = 0; i < index; i++)
            {
                if (Scroll.horizontal)
                    size += Grid.cellSize.x + Grid.spacing.x;
                else
                    size -= Grid.cellSize.y + Grid.spacing.y;
            }

            return size;
        }

        protected IEnumerator TweenMoveToPos(Vector2 pos, Vector2 v2Pos, float delay = 0.5f)
        {
            bool running = true;
            float passedTime = 0f;
            while (running)
            {
                yield return new WaitForEndOfFrame();
                passedTime += Time.deltaTime;
                Vector2 vCur;
                if (passedTime >= delay)
                {
                    vCur = v2Pos;
                    running = false;
                    if (m_Coroutine != null)
                    {
                        StopCoroutine(m_Coroutine);
                        m_Coroutine = null;
                    }
                    MoveFlag = false;
                    Scroll.StopMovement();
                }
                else
                {
                    vCur = Vector2.Lerp(pos, v2Pos, passedTime / delay);
                }

                Grid.rectTransform().anchoredPosition = vCur;
            }
        }
        #endregion

        private bool isdrag = false;
        public void OnBeginDrag(PointerEventData eventData)
        {
            isdrag = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isdrag = true;
        }
    }
}