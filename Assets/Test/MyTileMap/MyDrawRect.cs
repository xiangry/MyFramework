using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[Hotfix()]
[CSharpCallLua]
[LuaCallCSharp()]
public class MyDrawRect : Graphic
{
    public UIVertex[] vertexs = new UIVertex[4];
    public Color curColor = new Color(1,116/255,102/255,0.6f);
    protected override void Start()
    {
        base.Start();
        raycastTarget = false;
        SetRectColor(curColor);
    }


    public void SetGreenRectInfo(Vector3 size)
    {
        Debug.Log($"-----SetGreenRectInfo==sizesize=== ({size})");
        Vector2 pos = new Vector2(0, -size.y * 0.5f);
        vertexs[0].position =  new Vector3(-size.x * 0.5f, 0, 0);
        vertexs[1].position =  new Vector3(-size.x * 0.5f, size.y, 0);
        vertexs[2].position =  new Vector3(size.x * 0.5f, size.y, 0);
        vertexs[3].position =  new Vector3(size.x * 0.5f, 0, 0);
        
        Debug.Log($"-----SetGreenRectInfo===== ({vertexs[0].position})");
    }

    public void SetRect(Vector2 rect, Vector2 position)
    {
        Vector2 size = rect;
        Vector2 pos = position;
        vertexs[0].position =  new Vector3(pos.x,  pos.y, 0);
        vertexs[1].position =  new Vector3(pos.x + size.x, pos.y, 0);
        vertexs[2].position =  new Vector3(pos.x + size.x, pos.y - size.y, 0);
        vertexs[3].position =  new Vector3(pos.x,  pos.y - size.y, 0);
    }
    
    public void SetRectColor(Color color)
    {
        curColor = color;
        for (int i = 0; i < 4; i++)
        {
            vertexs[i].color = curColor;
        }
    }

    private void Update()
    {
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
//        base.OnPopulateMesh(vh);
        vh.Clear(); // 清除Graphic默认提供的正方形
        vh.AddUIVertexQuad(vertexs);
//        DrawLine(vh, vertexs[0].position, vertexs[1].position, 3, 1);
//        DrawLine(vh, vertexs[1].position, vertexs[2].position, 3, 2);
//        DrawLine(vh, vertexs[2].position, vertexs[3].position, 3, 1);
//        DrawLine(vh, vertexs[3].position, vertexs[0].position, 3, 2);
        
    }

    private void DrawLine(VertexHelper vh, Vector3 start, Vector3 end, int size, int lineType)
    {
        UIVertex[] vertexsL = new UIVertex[4];
        vertexsL[0].position = start;
        vertexsL[1].position = end;
        if (lineType == 2)
        {
            vertexsL[2].position = new Vector3(end.x , end.y + size, end.z);        
            vertexsL[3].position = new Vector3(start.x , start.y + size, start.z);
        }
        else
        {
            vertexsL[2].position = new Vector3(end.x + size, end.y, end.z);        
            vertexsL[3].position = new Vector3(start.x + size, start.y, start.z);
        }
        
        curColor = color;
        for (int i = 0; i < 4; i++)
        {
            vertexsL[i].color = new Color(vertexs[i].color.r, vertexs[i].color.g, vertexs[i].color.b, 1) ;
        }

        vh.AddUIVertexQuad(vertexsL);
    }
}