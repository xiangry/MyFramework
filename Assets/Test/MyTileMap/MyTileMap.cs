using System;
using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using XLua;

[CSharpCallLua]
[LuaCallCSharp()]
public class BlockTile
{
    public int id;

    public Vector2 size;
    public Vector2 position; // 以中心点为0，0点的坐标系
    public Vector2 realPos; // 以坐下角为0，0点的坐标系
    public Vector2 rectPos; // 绿色选中区域的位置
    public RawImage img;
    public Vector2 show_size;
    public MyDrawRect drawRect = null;
    
    
}

[CSharpCallLua]
[LuaCallCSharp()]
public class ColliderRect
{
    public Vector2 position;
    public Vector2 size;
    public BlockTile tile1;
    public BlockTile tile2;
}

[CSharpCallLua]
[LuaCallCSharp()]
public class MyTileMap : MonoBehaviour
{
    private GameObject drawRectPoolObj;
    private List<MyDrawRect> drawPool;

    public List<RawImage> baseTile = new List<RawImage>();
    public List<GameObject> layers = new List<GameObject>();
//    public Dictionary<string, List<Vector3>> allTileRectList = new Dictionary<string, List<Vector3>>();
    public RawImage selTile;
    public int selindex = -1; //选中了哪个家具

    List<BlockTile> allTiles = new List<BlockTile>(); // 所有的建筑
    List<BlockTile> colliderTileList = new List<BlockTile>(); // 相交的矩形
    List<MyDrawRect> colliderTileDrawRect = new List<MyDrawRect>(); // 相交的矩形列表

    private Vector2 lastPoint = Vector2.zero;


    private GameObject colliderParent;
    List<ColliderRect> colliderRect = new List<ColliderRect>(); // 相交个数
    List<MyDrawRect> colliderDrawRect = new List<MyDrawRect>(); // 相交产生的矩形

    void ColliderChange()
    {
        for (int i = 0; i < colliderRect.Count; i++)
        {
            ColliderRect cInfo = colliderRect[i];
            MyDrawRect rect;
            if (i < colliderDrawRect.Count)
            {
                rect = colliderDrawRect[i];
            }
            else
            {
                rect = GetFreeDrawRectObj();
                colliderDrawRect.Add(rect);
            }

            rect.transform.parent = colliderParent.transform;
            rect.transform.SetAsLastSibling();
            rect.transform.localPosition = Vector3.zero;
            rect.transform.localScale = Vector3.one;
            rect.SetRect(cInfo.size, cInfo.position);

            DrawTileRect(cInfo.tile1);
            DrawTileRect(cInfo.tile2);
        }

        for (int i = colliderRect.Count; i < colliderDrawRect.Count; i++)
        {
            RecycleDrawRectObj(colliderDrawRect[i]);
        }
        
//        DrawColliderTile();
    }

    public void ClearColliderRectIgnoreTile(BlockTile tile)
    {
        for (int i = 0; i < colliderRect.Count; i++)
        {
            ColliderRect cInfo = colliderRect[i];
            if (tile != cInfo.tile1)
            {
                RecycleTileRectByTile(cInfo.tile1);
            }
            if (tile != cInfo.tile2)
            {
                RecycleTileRectByTile(cInfo.tile2);
            }
        }
    }

    public void DrawColliderTile()
    {
        for (int i = 0; i < colliderTileList.Count; i++)
        {
            if (allTiles[selindex].drawRect == null || colliderTileList[i].id != allTiles[selindex].id)
            {
                MyDrawRect rect;
                if (i < colliderTileDrawRect.Count)
                {
                    rect = colliderTileDrawRect[i];
                }
                else
                {
                    rect = GetFreeDrawRectObj();
                    colliderTileDrawRect.Add(rect);
                }
                rect.SetGreenRectInfo(colliderTileList[i].size);
                rect.transform.parent = colliderTileList[i].img.transform;
                rect.transform.localPosition = new Vector3(0,-colliderTileList[i].size.y/2);
                rect.transform.localScale = Vector3.one;
                rect.SetRectColor(new Color(0,1,0,0.2f));
            }
            
//            RecycleDrawRectObj(allTiles[selindex].drawRect);
        }
        Debug.Log($"colliderRect.Count ==={(colliderTileList.Count, colliderTileDrawRect.Count)}");
        
        for (int i = colliderTileList.Count; i < colliderTileDrawRect.Count; i++)
        {
            RecycleDrawRectObj(colliderTileDrawRect[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        colliderParent = GameObject.Find("UIRoot/LuanchLayer/tile_test/content/item");
        
        InitDrawRectPool();
        
        InitFloor();

        EasyTouch.On_TouchStart += OnTouchStart;
        EasyTouch.On_TouchUp += OnTouchEnd;
        EasyTouch.On_SwipeStart += EasyTouch_On_SwipeBegin;
        EasyTouch.On_Swipe += EasyTouch_On_Swipe;
        EasyTouch.On_SwipeEnd += EasyTouch_On_SwipeEnd;
    }

    public void InitFloor()
    {
        // 底板 纵向0-300 分6块 每块50 像素 order从下到上10 - 15  
        // 例如：单位格子大小是50*50 宽高为item中心点坐标上下左右各25
        
//        var layer = layers[1].transform;
//
//        RawImage[] tiles = layer.GetComponentsInChildren<RawImage>();
//        foreach (var t in tiles)
//        {
//            DestroyImmediate(t.gameObject);
//        }

        
//        for (int i = 0; i < 2; i++)
//        {
//            var tile1 = baseTile[i+4];
//            RawImage img = GameObject.Instantiate(tile1, layer);
//            img.transform.localPosition = new Vector3(-100f* i + i*-150, -100f + i*-150, 0f);
//            img.transform.name = "item"+i;
//            float width = img.transform.rectTransform().rect.width;
//            float height = img.transform.rectTransform().rect.height;
//            BlockTile tile = new BlockTile();
//            tile.img = img;
//            tile.size = new Vector2(width,height);
//            tile.show_size.x = width;
//            tile.show_size.y = 50;
//            tile.position = new Vector2(img.transform.localPosition.x,img.transform.localPosition.y);
//            tile.realPos = new Vector2(img.transform.localPosition.x + 640 ,img.transform.localPosition.y+320 + tile.show_size.y);
//            tile.id = i + 1;
////            allTileRectList[Convert.ToString(tile.id)] = tile.show_size;
//            allTiles.Add(tile);
//        }
        
    }

    // 算法：0，0，0 点为 640 , 360
    public Vector2 FloorPointChange(Vector2 localPoint)
    {
        Vector3 localPosition = new Vector3(0f , 0f, 0f);
        localPosition.x = localPoint.x - 640;
        localPosition.y = localPoint.y - 360;
        return localPosition;
    }
    
    
    
    int CheckClickOneTile(Vector2 gesture)
    {
        Vector2 size;
        Vector2 pos;
        for (int i = 0; i < allTiles.Count; i++)
        {
            size = allTiles[i].size;
            pos = allTiles[i].position;
            if ((pos.x - size.x/2)<gesture.x && (pos.x + size.x/2)> gesture.x && 
                (pos.y - size.y/2)<gesture.y && (pos.y + size.y/2)> gesture.y)
            {
                Debug.Log($"allTiles[i].id ({allTiles[i].id})");
                return i;
            }
        }
        
        return -1;
    }

    // 判断是否有矩形重叠
    void CheckSomeTileOverlapping(List<ColliderRect> colliderRect)
    {
        colliderRect.Clear();
        colliderTileList.Clear();
        // 当前点击的矩形的左上角坐标 左下角0，0坐标系y+矩形位置的/2+矩形大小的一半
        float curX1 = allTiles[selindex].realPos.x - allTiles[selindex].size.x / 2;
        float curY1 = allTiles[selindex].realPos.y - allTiles[selindex].size.y / 2 +
                      allTiles[selindex].show_size.y;
        // 当前点击的矩形的右下角坐标
        float curX2 = allTiles[selindex].realPos.x + allTiles[selindex].size.x / 2;
        float curY2 = allTiles[selindex].realPos.y - allTiles[selindex].size.y / 2;
        
//        Debug.Log($"curX1===curY1=----{(curX1, curY1)}");
//        Debug.Log($"curX2===curY2=----{(curX2, curY2)}");
        for (int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i].id != allTiles[selindex].id)
            {
                float x1 = allTiles[i].realPos.x - allTiles[i].size.x / 2; // 矩形左上角的x坐标
                float y1 = allTiles[i].realPos.y - allTiles[i].size.y / 2 +
                           allTiles[i].show_size.y; // 矩形左上角的y坐标

                float x2 = allTiles[i].realPos.x + allTiles[i].size.x / 2; // 矩形右下角的x坐标
                float y2 = allTiles[i].realPos.y - allTiles[i].size.y / 2; //矩形右下角的y坐标

                float StartX = Math.Min(x1, curX1); 
                float EndX = Math.Max(x2, curX2); 
                float StartY = Math.Max(y1, curY1); 
                float EndY = Math.Min(y2, curY2); 
                float CurWidth = (x2 - x1) + (curX2 - curX1) - (EndX - StartX); // 宽度 
                float CurHeight = (y1 - y2) + (curY1 - curY2) - (StartY- EndY); // 高度 

                if (CurWidth <= 0 || CurHeight <= 0)
                {
//                Debug.Log(" ------没有相交--------------- ");
                }
                else
                {
//                    Debug.Log(" ------相交--------------- ");
                    float tempX = Math.Max(x1, curX1); // 有相交则相交区域位置为：小中取大为左上角，大中取小为右下角
                    float tempY = Math.Min(y1, curY1);
                    
                    Vector2 curPos = FloorPointChange(new Vector2(tempX, tempY));
                    ColliderRect rect = new ColliderRect();
                    rect.position = curPos;
                    rect.size = new Vector2(CurWidth, CurHeight);
                    rect.tile1 = allTiles[selindex];
                    rect.tile2 = allTiles[i];
                    colliderRect.Add(rect);
                    colliderTileList.Add(allTiles[i]);
                    colliderTileList.Add(allTiles[selindex]);
                }
            }
        }

    }

    void OnTouchStart(Gesture gesture)
    {
        Debug.Log($"gesture ({gesture.position.x}, {gesture.position.y})");
        Vector2 localPoint;
        Vector3 localPosition;
        int index; //选中第几个建筑
        localPoint.x = gesture.position.x;
        localPoint.y = gesture.position.y;
        localPosition = FloorPointChange(localPoint);
        Debug.Log($"localPosition ({localPosition.x}, {localPosition.y})");
        index = CheckClickOneTile(localPosition);
        Debug.Log($"ididid ({index})");
        if (index != -1)
        {
            selindex = index;
            selTile = allTiles[index].img; // 获取选中的图片
            allTiles[index].realPos = localPoint;
            DrawTileRect(allTiles[index]);
        }
        
    }

    void OnTouchEnd(Gesture gesture)
    {
        Debug.Log($"OnTouchEnd--------- ({selTile})");
        if (selTile)
        {
            RecycleDrawRectObj(allTiles[selindex].drawRect);
            allTiles[selindex].drawRect = null;
            selTile = null;
            selindex = -1;
        }
    }

    void EasyTouch_On_SwipeBegin(Gesture gesture)
    {
        Debug.Log($"EasyTouch_On_SwipeBegin ({gesture.position.x}, {gesture.position.y})");
        if (selTile)
        {
            Vector3 localPosition;
            Vector2 localPoint;
            localPoint.x = gesture.position.x;
            localPoint.y = gesture.position.y;
            localPosition = FloorPointChange(localPoint);
            selTile.transform.localPosition = localPosition;
        }
        
    }


    void EasyTouch_On_Swipe(Gesture gesture)
    {
//        Debug.Log($"EasyTouch_On_Swipe ({gesture.position.x}, {gesture.position.y})");
        if (selTile)
        {
            if (gesture.position.Equals(lastPoint))
            {
                return;
            }

            lastPoint = gesture.position;
            
            // 图片实际大小的一半 - 所占地板的高度 + 地板的高度（300）= 实际在地板上的位置
            float tempY = allTiles[selindex].size.y / 2 - allTiles[selindex].drawRect.vertexs[1].position.y;
//            Debug.Log($"tempY======= ({tempY})");
            if (gesture.position.y - tempY <= 300 && gesture.position.y - tempY > 0)
            {
                Vector3 localPosition;
                Vector2 localPoint;
                localPoint.x = gesture.position.x;
                localPoint.y = gesture.position.y;
                localPosition = FloorPointChange(localPoint);
                selTile.transform.localPosition = localPosition;
                allTiles[selindex].position = localPosition;
                allTiles[selindex].realPos = localPoint;
                int order = Convert.ToInt32(Math.Floor(gesture.position.y / 50));
//                Debug.Log($"order===== ({order})");
                selTile.transform.SetSiblingIndex(order);
            }
            if (gesture.position.y - tempY > 300)
            {
                Vector3 localPosition;
                Vector2 localPoint;
                localPoint.x = gesture.position.x;
                localPoint.y = 300+tempY;
                localPosition = FloorPointChange(localPoint);
                allTiles[selindex].position = localPosition;
                allTiles[selindex].realPos = localPoint;
                selTile.transform.localPosition = localPosition;
                selTile.transform.SetSiblingIndex(1);
            }

            if (gesture.position.y - tempY<=0)
            {
                Vector3 localPosition;
                Vector2 localPoint;
                localPoint.x = gesture.position.x;
                localPoint.y = tempY+allTiles[selindex].show_size.y;
                localPosition = FloorPointChange(localPoint);
                allTiles[selindex].position = localPosition;
                allTiles[selindex].realPos = localPoint; 
                selTile.transform.localPosition = localPosition;
                selTile.transform.SetSiblingIndex(6);
            }
            
            CheckSomeTileOverlapping(colliderRect);
            ColliderChange();
        }
    }

    void EasyTouch_On_SwipeEnd(Gesture gesture)
    {
        Debug.Log($"EasyTouch_On_SwipeEnd ({gesture.position.x}, {gesture.position.y})");
        if (selTile)
        {
            Vector3 localPosition;
            Vector2 localPoint;
            localPoint.x = gesture.position.x;
            localPoint.y = gesture.position.y;
            localPosition = FloorPointChange(localPoint);
            allTiles[selindex].position = localPosition;
            allTiles[selindex].realPos = localPoint;
            RecycleTileRectByTile(allTiles[selindex]);
            
//            CheckSomeTileOverlapping(colliderRect);
            ColliderChange();
            
            selTile = null;
            selindex = -1;
        }
    }

    // 画矩形
    public void DrawTileRect(BlockTile tile)
    {
        if (tile.drawRect==null)
        {
            MyDrawRect drawRect = GetFreeDrawRectObj();
            drawRect.SetGreenRectInfo(tile.size);
            drawRect.transform.parent = tile.img.transform;
            drawRect.transform.localPosition = new Vector3(0,-tile.size.y/2);
            drawRect.transform.localScale = Vector3.one;
            drawRect.SetRectColor(new Color(0,1,0,0.2f));
            tile.drawRect = drawRect;
        }
    }
    
    // 回收所画矩形
    public void RecycleTileRectByTile(BlockTile tile)
    {
        if (tile.drawRect!=null)
        {
            RecycleDrawRectObj(tile.drawRect);
            tile.drawRect = null;
        }
    }
    
    // 获取屏幕坐标系（左下角为0，0）转换为世界坐标系（中心点为0，0）
    public Rect ConvertPosSize2Rect(Vector2 position, Vector2 size)
    {
        Rect rect = new Rect();
        rect.position = FloorPointChange(position);
        rect.size = size;
        return rect;
    }
    // 判断两个家具是否相交
    public bool IsTileOverlap(BlockTile tile1, BlockTile tile2)
    {
        // 当前点击的矩形的左上角坐标 左下角0，0坐标系y+矩形位置的/2+矩形大小的一半
        float curX1 = tile1.realPos.x - tile1.size.x / 2;
        float curY1 = tile1.realPos.y - tile1.size.y / 2 + tile1.show_size.y;
        
        // 当前点击的矩形的右下角坐标
        float curX2 = tile1.realPos.x + tile1.size.x / 2;
        float curY2 = tile1.realPos.y - tile1.size.y / 2;

        bool flag = false;
        if (tile1.id != tile2.id)
        {
            float x1 = tile2.realPos.x - tile2.size.x / 2; // 矩形左上角的x坐标
            float y1 = tile2.realPos.y - tile2.size.y / 2 +
                       tile2.show_size.y; // 矩形左上角的y坐标

            float x2 = tile2.realPos.x + tile2.size.x / 2; // 矩形右下角的x坐标
            float y2 = tile2.realPos.y - tile2.size.y / 2; //矩形右下角的y坐标

            float StartX = Math.Min(x1, curX1); 
            float EndX = Math.Max(x2, curX2); 
            float StartY = Math.Max(y1, curY1); 
            float EndY = Math.Min(y2, curY2); 
            float CurWidth = (x2 - x1) + (curX2 - curX1) - (EndX - StartX); // 宽度 
            float CurHeight = (y1 - y2) + (curY1 - curY2) - (StartY- EndY); // 高度 

            if (CurWidth <= 0 || CurHeight <= 0)
            {
//                Debug.Log(" ------没有相交--------------- ");
                flag = false;
            }
            else
            {
                flag = true;
            }
        }

        return flag;
    }
    // 返回两个家具相交的矩形的左上角的点和矩形范围
    public Rect TileOverlapRect(BlockTile tile1, BlockTile tile2)
    {
        // 当前点击的矩形的左上角坐标 左下角0，0坐标系y+矩形位置的/2+矩形大小的一半
        float curX1 = tile1.realPos.x - tile1.size.x / 2;
        float curY1 = tile1.realPos.y - tile1.size.y / 2 + tile1.show_size.y;
        
        // 当前点击的矩形的右下角坐标
        float curX2 = tile1.realPos.x + tile1.size.x / 2;
        float curY2 = tile1.realPos.y - tile1.size.y / 2;

        Rect rect = new Rect();
        if (tile1.id != tile2.id)
        {
            float x1 = tile2.realPos.x - tile2.size.x / 2; // 矩形左上角的x坐标
            float y1 = tile2.realPos.y - tile2.size.y / 2 +
                       tile2.show_size.y; // 矩形左上角的y坐标

            float x2 = tile2.realPos.x + tile2.size.x / 2; // 矩形右下角的x坐标
            float y2 = tile2.realPos.y - tile2.size.y / 2; //矩形右下角的y坐标

            float StartX = Math.Min(x1, curX1); 
            float EndX = Math.Max(x2, curX2); 
            float StartY = Math.Max(y1, curY1); 
            float EndY = Math.Min(y2, curY2); 
            float CurWidth = (x2 - x1) + (curX2 - curX1) - (EndX - StartX); // 宽度 
            float CurHeight = (y1 - y2) + (curY1 - curY2) - (StartY- EndY); // 高度 

            if (CurWidth <= 0 || CurHeight <= 0)
            {
//                Debug.Log(" ------没有相交--------------- ");
            }
            else
            {
                float tempX = Math.Max(x1, curX1); // 有相交则相交区域位置为：小中取大为左上角，大中取小为右下角
                float tempY = Math.Min(y1, curY1);
                Vector2 curPos = FloorPointChange(new Vector2(tempX, tempY));
                rect.position = curPos;
                rect.size = new Vector2(CurWidth, CurHeight);
            }
        }

        return rect;
    }

    #region MyRegion 画矩形框对象池管理

    void InitDrawRectPool()
        {
            drawPool = new List<MyDrawRect>();
            GameObject pool = new GameObject("__DrawRectPool__");
            pool.SetActive(false);
            drawRectPoolObj = pool;
        }
        
        // 回收池
        MyDrawRect GetFreeDrawRectObj()
        {
            MyDrawRect drawRect = null;
            if (drawPool.Count > 0)
            {
                drawRect = drawPool[drawPool.Count - 1];
                drawPool.RemoveAt(drawPool.Count - 1);
            }
            else
            {
                GameObject obj = new GameObject("draw");
                drawRect = obj.AddComponent<MyDrawRect>();
                
            }
    
            return drawRect;
        }
    
        // 加入回收
        void RecycleDrawRectObj(MyDrawRect rect)
        {
            drawPool.Add(rect);
            rect.transform.SetParent(drawRectPoolObj.transform);
        }

    #endregion
}


#if UNITY_EDITOR
public static class MyTileMapExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(Vector2),
                typeof(Vector3),
        };
}
#endif