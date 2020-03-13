using System;
using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;

public class TestCamera : MonoBehaviour
{
     public bool isCamRotate = true;

    public Camera _mainCamera;
    public Vector3 centerPoint;
    public float rotateSpeed  = 1;
    public float maxRotSpeed1 = 4.0f;
    public float maxRotSpeed2 = -4.0f;
    public float endSpeed = 0.05f;
    float angle = 0;

    float idxMouX = 0;

    Vector3 v_trans ;
    Vector3 v_rotate ;
    // Start is called before the first frame update
    
    
    void Awake()
    {
        _mainCamera = gameObject.GetComponent<Camera>();
        v_trans = _mainCamera.transform.position;

        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
        
        EasyTouch.On_DragStart += EasyTouch_On_DragStart;
        EasyTouch.On_Drag += EasyTouch_On_Drag;
        EasyTouch.On_DragEnd += EasyTouch_On_DragEnd;
    }

    
    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        Debug.Log($"EasyTouch_On_TouchStart ---- ({gesture.position.x}, {gesture.position.y})");
        
        Ray ray = Camera.main.ScreenPointToRay(gesture.position);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
            Transform transform = hit.transform;

            GameObject areaObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            MeshRenderer render = areaObj.GetComponent<MeshRenderer>();
            render.material.shader = Shader.Find("Sprites/Diffuse");
            render.material.color = new Color(1f, 0f,0f, 0.1f);
            areaObj.transform.parent = transform;
            areaObj.transform.localPosition = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 开始拖
    /// </summary>
    /// <param name="gesture"></param>
    private void EasyTouch_On_DragStart(Gesture gesture)
    {
    }
    
    private void EasyTouch_On_DragEnd(Gesture gesture)
    {
    }
    
    private void EasyTouch_On_Drag(Gesture gesture)
    {
        idxMouX = gesture.deltaPosition.x;
        if(idxMouX > maxRotSpeed1)
            idxMouX = maxRotSpeed1;
        else if(idxMouX < maxRotSpeed2)
            idxMouX = maxRotSpeed2;
        CameraRotate(idxMouX);
    }
    
    /// <summary>
    /// 左键控制旋转
    /// </summary>
    /// <param name="_mouseX"></param>
    /// <param name="_mouseY"></param>
    public void CameraRotate(float _mouseX)
    {
        //注意!!! 此处是 GetMouseButton() 表示一直长按鼠标左键；不是 GetMouseButtonDown()
        
        //控制相机绕中心点(centerPoint)水平旋转
        _mainCamera.transform.RotateAround(centerPoint, Vector3.up, _mouseX * rotateSpeed);            

        //控制相机绕中心点垂直旋转(！注意此处的旋转轴时相机自身的x轴正方向！)
        //_mainCamera.transform.RotateAround(centerPoint, _mainCamera.transform.right, _mouseY * rotateSpeed);
    }
    
    private void OnDestroy()
    {
        EasyTouch.On_DragStart -= EasyTouch_On_DragStart;
        EasyTouch.On_Drag -= EasyTouch_On_Drag;
        EasyTouch.On_DragEnd -= EasyTouch_On_DragEnd;
    }
}
