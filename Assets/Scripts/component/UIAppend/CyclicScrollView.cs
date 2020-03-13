using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using XLua;
//*****************************-》 基类 循环列表 《-****************************
//author kim
//初始化:
//      Init(callBackFunc)
//刷新整个列表（首次调用和数量变化时调用）:
//      ShowList(int = 数量)
//刷新单个项:
//      UpdateCell(int = 索引)
//刷新列表数据(无数量变化时调用):
//      UpdateList()
//回调:
//Func(GameObject = Cell, int = Index)  //刷新列表
namespace UICyclicScrollView
{
    public class UIUtils
        {
            public static void SetActive(GameObject obj, bool isActive)
            {
                if (obj != null)
                {
                    obj.SetActive(isActive);
                }

            }
        }
    public enum e_Direction
        {
            Horizontal,
            Vertical
        }
    public enum e_Deflect
    {
        LeftorDown,
        RightorUp
    }
    public enum e_SortPivot
    {
        LeftUP,
        RightUporLeftDown,
    }


    [LuaCallCSharp()]
    [CSharpCallLua]
    public class CyclicScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public e_Direction m_Direction = e_Direction.Horizontal;
        public int m_Row = 1; //行或列
        public float m_Spacing_x = 0f; //横向间距
        public float m_Spacing_y = 0f; //纵向向间距
        public GameObject m_CellGameObject; //指定的cell

        public GameObject m_PointingFirstArrow;
        public GameObject m_PointingEndArrow;
        public bool m_IsShowArrow = true;

        public bool m_IsDeflect = false; //是否进行偏斜
        public e_Deflect m_Deflect = e_Deflect.LeftorDown; //偏斜方向
        public float m_DeflectAngle = 0; //偏斜角度
        public e_SortPivot m_SortPivot = e_SortPivot.LeftUP; //对齐锚点


        protected Action<int, GameObject> m_FuncCallBackFunc;
        protected Action<int,GameObject> m_FuncOnClickCallBack;
        [CSharpCallLua]
        //public delegate void EClickRefresh(int index,bool bo,GameObject go);
        //protected EClickRefresh m_FuncOnButtonClickCallBack;
        protected Action<int, bool, GameObject> m_FuncOnButtonClickCallBack;

        protected RectTransform rectTrans;

        protected float m_PlaneWidth;
        protected float m_PlaneHeight;

        protected float m_ContentWidth;
        protected float m_ContentHeight;

        protected float m_CellObjectWidth;
        protected float m_CellObjectHeight;

        protected GameObject m_Content;
        protected RectTransform m_ContentRectTrans;

        private bool m_isInited = false;

        //记录 物体的坐标 和 物体 
        protected struct CellInfo
        {
            public Vector3 pos;
            public GameObject obj;
        };
        protected CellInfo[] m_CellInfos;

        protected bool m_IsInited = false;

        protected ScrollRect m_ScrollRect;

        protected int m_MaxCount = -1; //列表数量

        protected int m_MinIndex = -1;
        protected int m_MaxIndex = -1;

        protected bool m_IsClearList = false; //是否清空列表

        private Coroutine m_Coroutine;//当前进行的协程

        public virtual void Init(Action<int, GameObject> callBack)
        {
            Init(callBack, null);
        }
        public virtual void Init(Action<int, GameObject> callBack, Action<int, GameObject> onClickCallBack, Action<int, bool, GameObject> onButtonClickCallBack)
            {
                if (onButtonClickCallBack != null)
                {
                    m_FuncOnButtonClickCallBack = onButtonClickCallBack;
                }
                Init(callBack, onClickCallBack);
            }
        public virtual void Init(Action<int, GameObject> callBack, Action<int, GameObject> onClickCallBack)
        {
            DisposeAll();

            m_FuncCallBackFunc = callBack;

            if (onClickCallBack != null)
            {
                m_FuncOnClickCallBack = onClickCallBack;
            }

            if (m_isInited)
                return;


            m_Content = this.GetComponent<ScrollRect>().content.gameObject;

            if (m_CellGameObject == null)
            {
                m_CellGameObject = m_Content.transform.GetChild(0).gameObject;
            }
            /* Cell 处理 */
            SetPoolsObj(m_CellGameObject);
            RectTransform cellRectTrans = m_CellGameObject.GetComponent<RectTransform>();
            cellRectTrans.pivot =  new Vector2(0f, 1f);
            CheckCellAnchor(cellRectTrans);
            cellRectTrans.anchoredPosition3D = Vector3.zero;

            //记录 Cell 信息
            m_CellObjectHeight = cellRectTrans.rect.height;
            m_CellObjectWidth = cellRectTrans.rect.width;

            //记录 Plane 信息
            rectTrans = GetComponent<RectTransform>();
            Rect planeRect = rectTrans.rect;
            m_PlaneHeight = planeRect.height;
            m_PlaneWidth = planeRect.width;

            //记录 Content 信息
            m_ContentRectTrans = m_Content.GetComponent<RectTransform>();
            Rect contentRect = m_ContentRectTrans.rect;
            m_ContentHeight = contentRect.height;
            m_ContentWidth = contentRect.width;

            m_ContentRectTrans.pivot = new Vector2(0f, 1f);
            //m_ContentRectTrans.sizeDelta = new Vector2 (planeRect.width, planeRect.height);
            //m_ContentRectTrans.anchoredPosition = Vector2.zero;
            CheckAnchor(m_ContentRectTrans);

            m_ScrollRect = this.GetComponent<ScrollRect>();
            m_ScrollRect.onValueChanged.RemoveAllListeners();
            //添加滑动事件
            m_ScrollRect.onValueChanged.AddListener(delegate (Vector2 value) { ScrollRectListener(value); });

            if (m_PointingFirstArrow != null || m_PointingEndArrow != null)
            {
                m_ScrollRect.onValueChanged.AddListener(delegate (Vector2 value) { OnDragListener(value); });
                OnDragListener(Vector2.zero);
            }

            if (m_IsDeflect)
            {
                m_ScrollRect.onValueChanged.AddListener(delegate (Vector2 value) { UpdateContentPos(value); });
                UpdateContentPos(Vector2.zero);
            }
            m_isInited = true;
        }
        //检查 Content Anchor 是否正确
        private void CheckAnchor(RectTransform rectTrans)
            {
            if (m_Direction == e_Direction.Vertical)
            {
                if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                         (rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(1, 1))))
                {
                    rectTrans.anchorMin = new Vector2(0, 1);
                    rectTrans.anchorMax = new Vector2(1, 1);
                }
            }
            else
            {
                if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                         (rectTrans.anchorMin == new Vector2(0, 0) && rectTrans.anchorMax == new Vector2(0, 1))))
                {
                    rectTrans.anchorMin = new Vector2(0, 0);
                    rectTrans.anchorMax = new Vector2(0, 1);
                }
            }
            
        }
        //检查cellanchor
        private void CheckCellAnchor(RectTransform rectTrans)
        {
            Vector2 now_Anchor;
            if (m_IsDeflect)
            {
                if (m_Direction == e_Direction.Vertical)
                {
                    now_Anchor = m_SortPivot == e_SortPivot.LeftUP ? new Vector2(0, 1) : new Vector2(1, 1);
                }
                else
                {
                    now_Anchor = m_SortPivot == e_SortPivot.LeftUP ? new Vector2(0, 1) : new Vector2(0, 0);
                }
            }
            else
            {
                now_Anchor = new Vector2(0, 1);
            }
                
            rectTrans.anchorMin = now_Anchor;
            rectTrans.anchorMax = now_Anchor;
        }

        //实时刷新列表时用
        public virtual void UpdateList()
            {
                for (int i = 0, length = m_CellInfos.Length; i < length; i++)
                {
                    CellInfo cellInfo = m_CellInfos[i];
                    if (cellInfo.obj != null)
                    {
                        float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                        if (!IsOutRange(rangePos))
                        {
                            Func(m_FuncCallBackFunc, cellInfo.obj, true);
                        }
                    }
                }
            }

            //刷新某一项
        public void UpdateCell(int index)
            {
                CellInfo cellInfo = m_CellInfos[index - 1];
                if (cellInfo.obj != null)
                {
                    float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                    if (!IsOutRange(rangePos))
                    {
                        Func(m_FuncCallBackFunc, cellInfo.obj);
                    }
                }
            }

        public virtual void ShowList(string numStr) { }

        public virtual void ShowList(int num)
        {
            m_MinIndex = -1;
            m_MaxIndex = -1;
            m_ScrollRect.StopMovement();
            //-> 计算 Content 尺寸
            if (m_Direction == e_Direction.Vertical)
            {
                float contentSize = (m_Spacing_y + m_CellObjectHeight) * Mathf.CeilToInt((float)num / m_Row);
                m_ContentHeight = contentSize;
                m_ContentWidth = m_ContentRectTrans.sizeDelta.x;
                contentSize = contentSize < rectTrans.rect.height ? rectTrans.rect.height : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(m_ContentWidth, contentSize);
                if (num != m_MaxCount)
                {
                    //m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, 0);
                    m_ContentRectTrans.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                float contentSize = (m_Spacing_x + m_CellObjectWidth) * Mathf.CeilToInt((float)num / m_Row);
                m_ContentWidth = contentSize;
                m_ContentHeight = m_ContentRectTrans.sizeDelta.y;
                contentSize = contentSize < rectTrans.rect.width ? rectTrans.rect.width : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(contentSize, m_ContentHeight);
                if (num != m_MaxCount)
                {
                    //m_ContentRectTrans.anchoredPosition = new Vector2(0, m_ContentRectTrans.anchoredPosition.y);
                    m_ContentRectTrans.anchoredPosition = Vector2.zero;
                }
            }

            //-> 计算 开始索引
            int lastEndIndex = 0;

            //-> 过多的物体 扔到对象池 ( 首次调 ShowList函数时 则无效 )
            if (m_IsInited)
                {
                    lastEndIndex = num - m_MaxCount > 0 ? m_MaxCount : num;
                    lastEndIndex = m_IsClearList ? 0 : lastEndIndex;

                    int count = m_IsClearList ? m_CellInfos.Length : m_MaxCount;
                    for (int i = lastEndIndex; i < count; i++)
                    {
                        if (m_CellInfos[i].obj != null)
                        {
                            SetPoolsObj(m_CellInfos[i].obj);
                            m_CellInfos[i].obj = null;
                        }
                    }
                }

            //-> 以下四行代码 在for循环所用
            CellInfo[] tempCellInfos = m_CellInfos;
            m_CellInfos = new CellInfo[num];

            //-> 1: 计算 每个Cell坐标并存储 2: 显示范围内的 Cell
            for (int i = 0; i < num; i++)
            {
                // * -> 存储 已有的数据 ( 首次调 ShowList函数时 则无效 )
                if (m_MaxCount != -1 && i < lastEndIndex)
                {
                    CellInfo tempCellInfo = tempCellInfos[i];
                    //-> 计算是否超出范围
                    float rPos = m_Direction == e_Direction.Vertical ? tempCellInfo.pos.y : tempCellInfo.pos.x;
                    if (!IsOutRange(rPos))
                        {
                            //-> 记录显示范围中的 首位index 和 末尾index
                            m_MinIndex = m_MinIndex == -1 ? i : m_MinIndex; //首位index
                            m_MaxIndex = i; // 末尾index

                            if (tempCellInfo.obj == null)
                            {
                                tempCellInfo.obj = GetPoolsObj();
                            }
                            tempCellInfo.obj.transform.GetComponent<RectTransform>().anchoredPosition3D = tempCellInfo.pos;
                            tempCellInfo.obj.name = (i+1).ToString();
                            tempCellInfo.obj.SetActive(true);

                            Func(m_FuncCallBackFunc, tempCellInfo.obj);
                        }
                    else
                    {
                        SetPoolsObj(tempCellInfo.obj);
                        tempCellInfo.obj = null;
                    }
                    m_CellInfos[i] = tempCellInfo;
                    continue;
                }

                CellInfo cellInfo = new CellInfo();

                float pos = 0;  //坐标( isVertical ? 记录Y : 记录X )
                float rowPos = 0; //计算每排里面的cell 坐标

                // * -> 计算每个Cell坐标 --cell中心在左上角
                if (m_Direction == e_Direction.Vertical)
                {
                    pos = m_CellObjectHeight * Mathf.FloorToInt(i / m_Row) + m_Spacing_y * Mathf.FloorToInt(i / m_Row);
                    rowPos = m_CellObjectWidth * (i % m_Row) + m_Spacing_x * (i % m_Row);
                    if (m_IsDeflect)
                    {
                        float tan_f = pos * Mathf.Tan(m_DeflectAngle / 180 * Mathf.PI);
                        if (m_SortPivot == e_SortPivot.RightUporLeftDown)
                        {
                            int rowindex = m_Row - 1 - (i % m_Row);
                            rowPos = m_CellObjectWidth * (rowindex % m_Row) + m_Spacing_x * (rowindex % m_Row);
                            rowPos = -rowPos;
                        }
                        tan_f = m_Deflect == e_Deflect.LeftorDown ? -tan_f : tan_f;
                        rowPos = rowPos + tan_f;
                        rowPos = m_SortPivot == e_SortPivot.LeftUP ? rowPos : (-m_CellObjectWidth + rowPos);
                    }
                    cellInfo.pos = new Vector3(rowPos, -pos, 0);
                }
                else
                {
                    pos = m_CellObjectWidth * Mathf.FloorToInt(i / m_Row) + m_Spacing_x * Mathf.FloorToInt(i / m_Row);
                    rowPos = m_CellObjectHeight * (i % m_Row) + m_Spacing_y * (i % m_Row);
                    if (m_IsDeflect)
                    {
                        float tan_f = pos * Mathf.Tan(m_DeflectAngle / 180 * Mathf.PI);
                        if (m_SortPivot == e_SortPivot.RightUporLeftDown)
                        {
                            int rowindex = m_Row - 1 - (i % m_Row);
                            rowPos = m_CellObjectHeight * (rowindex % m_Row) + m_Spacing_y * (rowindex % m_Row);
                            rowPos = -rowPos;
                        }

                        tan_f = m_Deflect == e_Deflect.LeftorDown ? tan_f : -tan_f;
                        rowPos = rowPos + tan_f;
                        rowPos = m_SortPivot == e_SortPivot.LeftUP ? rowPos : - m_CellObjectHeight + rowPos;
                    }

                    cellInfo.pos = new Vector3(pos, -rowPos, 0);
                }

                //-> 计算是否超出范围
                float cellPos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                if (IsOutRange(cellPos))
                {
                    cellInfo.obj = null;
                    m_CellInfos[i] = cellInfo;
                    continue;
                }

                //-> 记录显示范围中的 首位index 和 末尾index
                m_MinIndex = m_MinIndex == -1 ? i : m_MinIndex; //首位index
                m_MaxIndex = i; // 末尾index

                //-> 取或创建 Cell
                GameObject cell = GetPoolsObj();
                RectTransform cell_rt = cell.transform.GetComponent<RectTransform>();
                cell.transform.GetComponent<RectTransform>().anchoredPosition3D = cellInfo.pos;
                cell.gameObject.name = (i+1).ToString();

                //-> 存数据
                cellInfo.obj = cell;
                m_CellInfos[i] = cellInfo;

                //-> 回调  函数
                Func(m_FuncCallBackFunc, cell);
            }

            m_MaxCount = num;
            m_IsInited = true;

            OnDragListener(Vector2.zero);
        }

        // 更新滚动区域的大小
        public void UpdateSize()
        {
            Rect rect = GetComponent<RectTransform>().rect;
            m_PlaneHeight = rect.height;
            m_PlaneWidth = rect.width;
        }

        //滑动事件
        protected virtual void ScrollRectListener(Vector2 value)
        {
            UpdateCheck();
        }

        private void UpdateCheck()
        {
            if (m_CellInfos == null)
                return;

            //检查超出范围
            for (int i = 0, length = m_CellInfos.Length; i < length; i++)
            {
                CellInfo cellInfo = m_CellInfos[i];
                GameObject obj = cellInfo.obj;
                Vector3 pos = cellInfo.pos;

                float rangePos = m_Direction == e_Direction.Vertical ? pos.y : pos.x;
                //判断是否超出显示范围
                if (IsOutRange(rangePos))
                {
                    //把超出范围的cell 扔进 poolsObj里
                    if (obj != null)
                    {
                        
                        SetPoolsObj(obj);
                        m_CellInfos[i].obj = null;
                    }
                }
                else
                {
                    if (obj == null)
                    {
                        //优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                        GameObject cell = GetPoolsObj();
                        //cell.transform.localPosition = pos;
                        cell.GetComponent<RectTransform>().anchoredPosition3D = cellInfo.pos;
                        cell.gameObject.name = (i+1).ToString();
                        m_CellInfos[i].obj = cell;

                        Func(m_FuncCallBackFunc, cell);
                    }
                }
            }
        }
        //刷新content位置
        protected void UpdateContentPos(Vector2 value)
        {
            if (m_Direction == e_Direction.Vertical)
            {
                float ant = Mathf.Tan(m_DeflectAngle/180*Mathf.PI);
                float set_x = m_ContentRectTrans.anchoredPosition.y * ant;
                set_x = m_Deflect == e_Deflect.RightorUp ? -set_x : set_x;
                m_ContentRectTrans.anchoredPosition = new Vector2(set_x, m_ContentRectTrans.anchoredPosition.y);
            }  
            else
            {
                float ant = Mathf.Tan(m_DeflectAngle / 180 * Mathf.PI);
                float set_y = -m_ContentRectTrans.anchoredPosition.x * ant;
                set_y = m_Deflect == e_Deflect.RightorUp ? -set_y : set_y;
                m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x,set_y);
            }
        }

        //判断是否超出显示范围
        protected bool IsOutRange(float pos)
        {
            Vector3 listP = m_ContentRectTrans.anchoredPosition;
            if (m_Direction == e_Direction.Vertical)
            {
                if (pos + listP.y > m_CellObjectHeight || pos + listP.y < -rectTrans.rect.height)
                {
                    return true;
                }
            }
            else
            {
                if (pos + listP.x < -m_CellObjectWidth || pos + listP.x > rectTrans.rect.width)
                {
                    return true;
                }
            }
            return false;
        }

        //对象池 机制  (存入， 取出) cell
        protected Stack<GameObject> poolsObj = new Stack<GameObject>();
        //取出 cell
        protected virtual GameObject GetPoolsObj()
        {
            GameObject cell = null;
            if (poolsObj.Count > 0)
            {
                cell = poolsObj.Pop();
            }

            if (cell == null)
            {
                cell = Instantiate(m_CellGameObject) as GameObject;
            }
            cell.transform.SetParent(m_Content.transform);
            cell.transform.localScale = Vector3.one;
            UIUtils.SetActive(cell, true);

            return cell;
        }
        //存入 cell
        protected virtual void SetPoolsObj(GameObject cell)
        {
            if (cell != null)
            {
                cell.gameObject.name = "item";
                poolsObj.Push(cell);
                UIUtils.SetActive(cell, false);
            }
        }

        //回调
        protected void Func(Action<int, GameObject> func, GameObject selectObject, bool isUpdate = false)
        {
            int num = int.Parse(selectObject.name);
            if (func != null)
            {
                func(num, selectObject);
            }

        }

        public void DisposeAll()
        {
            if (m_FuncCallBackFunc != null)
            {
                m_FuncCallBackFunc = null;
            }
            if (m_FuncOnClickCallBack != null)
            {
                m_FuncOnClickCallBack = null;
            }
        }

        protected void OnDestroy()
        {
            DisposeAll();
        }
        public virtual void OnClickExpand(int index) { }

        //-> FlipCircularScrollView 函数
        public virtual void SetToPageIndex(int index) { }
        public virtual void OnClickCell(GameObject cell) { }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {

        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {

        }

        protected void OnDragListener(Vector2 value)
        {
            float normalizedPos = m_Direction == e_Direction.Vertical ? m_ScrollRect.verticalNormalizedPosition : m_ScrollRect.horizontalNormalizedPosition;

            if (m_Direction == e_Direction.Vertical)
            {
                if (m_ContentHeight - rectTrans.rect.height < 10)
                {
                    UIUtils.SetActive(m_PointingFirstArrow, false);
                    UIUtils.SetActive(m_PointingEndArrow, false);
                    return;
                }
            }
            else
            {
                if (m_ContentWidth - rectTrans.rect.width < 10)
                {
                    UIUtils.SetActive(m_PointingFirstArrow, false);
                    UIUtils.SetActive(m_PointingEndArrow, false);
                    return;
                }
            }

            if (normalizedPos >= 0.9)
            {
                UIUtils.SetActive(m_PointingFirstArrow, false);
                UIUtils.SetActive(m_PointingEndArrow, true);
            }
            else if (normalizedPos <= 0.1)
            {
                UIUtils.SetActive(m_PointingFirstArrow, true);
                UIUtils.SetActive(m_PointingEndArrow, false);
            }
            else
            {
                UIUtils.SetActive(m_PointingFirstArrow, true);
                UIUtils.SetActive(m_PointingEndArrow, true);
            }

        }

        protected Vector2 GetMoveTarget(int index)
        {
            float ant =m_IsDeflect ? Mathf.Tan(m_DeflectAngle / 180 * Mathf.PI): 0;
            if ((m_MaxCount - index) < m_MaxIndex) //避免定位到指定位置再回弹的现象
            {
               
                if (m_Direction == e_Direction.Vertical)
                {
                    float set_y = m_ContentHeight - m_PlaneHeight;
                    float set_x = set_y * ant;
                    set_x = m_Deflect == e_Deflect.RightorUp ? -set_x : set_x;
                    return new Vector2(set_x, set_y);
                }
                else
                {
                    float set_x = m_ContentWidth - m_PlaneHeight;
                    float set_y = set_x * ant;
                    set_y = m_Deflect == e_Deflect.RightorUp ? -set_y : set_y;
                    return new Vector2(-set_x, set_y);
                }
            }

            CellInfo cellInfo = m_CellInfos[index - 1];
             
            float pos_size = m_Direction == e_Direction.Vertical ? -cellInfo.pos.y : cellInfo.pos.x;
            float long_size = m_Direction == e_Direction.Vertical ? m_ContentHeight : m_ContentWidth;
            float size_ratio = pos_size / long_size;
            if (m_Direction == e_Direction.Vertical)
            {
                float set_y = -cellInfo.pos.y;
                float set_x = set_y * ant;
                set_x = m_Deflect == e_Deflect.RightorUp ? -set_x : set_x;
                return new Vector2(set_x, set_y);


                //if (m_SortPivot == e_SortPivot.RightUporLeftDown)
                //{
                //     return new Vector2(-cellInfo.pos.x - m_CellObjectWidth, -cellInfo.pos.y);
                //}
                //else
                //{
                //    return new Vector2(-cellInfo.pos.x, -cellInfo.pos.y);
                //}
                //m_ScrollRect.verticalNormalizedPosition = 1- size_ratio;
            }
            else
            {
                float set_x = cellInfo.pos.x;
                float set_y = set_x * ant;
                set_y = m_Deflect == e_Deflect.RightorUp ? -set_y : set_y;
                return new Vector2(-set_x, set_y);

                //m_ScrollRect.horizontalNormalizedPosition = size_ratio;
                //if (m_SortPivot == e_SortPivot.RightUporLeftDown)
                //{
                //    return new Vector2(-cellInfo.pos.x, m_CellObjectHeight - cellInfo.pos.y);
                //}
                //else
                //{
                //    return new Vector2(-cellInfo.pos.x, -cellInfo.pos.y);
                //}
            }
        }


        public virtual void MoveToIndex(int index)
        {
            if (index<1 || index> m_MaxCount)
            {
                return;
            }
            if (m_Direction == e_Direction.Vertical && m_ContentHeight <= m_PlaneHeight)
            {
                return;   
            }
            if (m_Direction == e_Direction.Horizontal && m_ContentWidth <= m_PlaneWidth)
            {
                return;
            }
            
            m_ContentRectTrans.anchoredPosition = GetMoveTarget(index);
            
            UpdateCheck();
            m_ScrollRect.StopMovement();
        }
        public virtual void MoveToIndex(int index,float delay = 0.5f)
        {
            if (index < 1 || index > m_MaxCount)
            {
                return;
            }
            if (m_Direction == e_Direction.Vertical && m_ContentHeight <= m_PlaneHeight)
            {
                return;
            }
            if (m_Direction == e_Direction.Horizontal && m_ContentWidth <= m_PlaneWidth)
            {
                return;
            }
            Vector2 V2Pos = GetMoveTarget(index);
            m_Coroutine = StartCoroutine(TweenMoveToPos(m_ContentRectTrans.anchoredPosition, V2Pos, delay));
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
                    //MoveFlag = false;
                    m_ScrollRect.StopMovement();
                }
                else
                {
                    vCur = Vector2.Lerp(pos, v2Pos, passedTime / delay);
                }
                UpdateCheck();
                m_ContentRectTrans.anchoredPosition = vCur;
            }
        }
    }
}


#if UNITY_EDITOR
public static class CyclicScrollViewExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(Action<int,GameObject>),
        typeof(Action<int, bool, GameObject>),
        typeof(UICyclicScrollView.CyclicScrollView),
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(UICyclicScrollView.CyclicScrollView),
        typeof(Action<int,GameObject>),
        typeof(Action<int, bool, GameObject>),
    };
}
#endif