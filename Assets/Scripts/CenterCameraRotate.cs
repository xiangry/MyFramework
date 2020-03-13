using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;

public class CenterCameraRotate : MonoBehaviour
{
    public bool isCamRotate = true;

    public Camera _mainCamera;
    public Vector3 centerPoint;
    public float rotateSpeed  = 1;
    public float maxRotSpeed1 = 4.0f;
    public float maxRotSpeed2 = -4.0f;
    private float moveX = 0;

    
    void Awake()
    {
        _mainCamera = gameObject.GetComponent<Camera>();
        
        EasyTouch.On_Swipe += EasyTouch_On_Swipe;
    }
    
    private void EasyTouch_On_Swipe(Gesture gesture)
    {
        if (isCamRotate)
        {
            moveX = gesture.deltaPosition.x;
            if(moveX > maxRotSpeed1)
                moveX = maxRotSpeed1;
            else if(moveX < maxRotSpeed2)
                moveX = maxRotSpeed2;
            CameraRotate(moveX);
        }
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
        EasyTouch.On_Swipe -= EasyTouch_On_Swipe;
    }
}
