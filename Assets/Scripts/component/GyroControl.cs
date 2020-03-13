using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class GyroControl : MonoBehaviour
{
    private Camera camera = null;
    
    public Vector3 initEuler = Vector3.zero;                    //初始旋转对应欧拉角

    private Quaternion initRotation = Quaternion.identity;      //初始旋转信息
    
    private Vector3 curEuler = Vector3.zero;     //当前续传信息

    private Vector3 lastInitEuler = Vector3.zero;              //用于判断初始欧拉角是否变化

    public float vertRotateRateScale = 0.1f;                      //垂直陀螺仪旋转速率缩放系数

    public float horiRotateRateScale = 0.2f;                      //水平陀螺仪旋转速率缩放系数

    public float restoreRate = 0.05f;                              //回复速率

    private float maxRotateRate = 1;                    //陀螺仪单轴最大旋转速率

    private float factor = 0.1f;                                  //陀螺仪旋转速率和回复速率间的关联系数(旋转速率越大回复越快)

    private float vertRotateRate = 0f;                          //垂直方向旋转速率

    private float horiRotateRate = 0f;                          //水平方向旋转速率
    
    void Init()
    {

        initRotation = transform.localRotation;

        initEuler = initRotation.eulerAngles;

        lastInitEuler = initEuler;

        Input.gyro.enabled = true;

    }

    /// <summary>

    /// 根据陀螺仪控制主界面mainCamera的旋转

    /// </summary>

    void CameraRotateControl()

    {

        if (!Input.gyro.enabled)

        {

            return;

        }

        //如果初始旋转角度发生变化,则更新

        if (lastInitEuler != initEuler)

        {

            initRotation = Quaternion.Euler(initEuler);

            lastInitEuler = initEuler;

        }

        vertRotateRate = Input.gyro.rotationRateUnbiased.x;

        vertRotateRate = Mathf.Sign(vertRotateRate) * Mathf.Clamp(Mathf.Abs(vertRotateRate), 0, maxRotateRate);

        horiRotateRate = Input.gyro.rotationRateUnbiased.y;

        horiRotateRate = Mathf.Sign(horiRotateRate) * Mathf.Clamp(Mathf.Abs(horiRotateRate), 0, maxRotateRate);

        factor = Mathf.Max(Mathf.Abs(vertRotateRate), Mathf.Abs(horiRotateRate)) / maxRotateRate;

        //z轴不旋转
        transform.Rotate(vertRotateRateScale * vertRotateRate, horiRotateRateScale * horiRotateRate, 0);

        //逐帧往初始位置回复
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initRotation, restoreRate + restoreRate * factor);
    }
    

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        CameraRotateControl();
    }

//    private void OnGUI()
//    {
//        GUILayout.Label("rotation rate unbiased:" + Input.gyro.rotationRateUnbiased);
//
//        vertRotateRateScale = GUI.HorizontalSlider(new Rect(75, 400, 200, 40), vertRotateRateScale, 0f, 1f);
//
//        GUI.Label(new Rect(300, 395, 200, 40), "Vert Rotate Rate:" + vertRotateRateScale);
//
//        horiRotateRateScale = GUI.HorizontalSlider(new Rect(75, 475, 200, 40), horiRotateRateScale, 0f, 1f);
//
//        GUI.Label(new Rect(300, 470, 200, 40), "Hori Rotate Rate:" + horiRotateRateScale);
//
//        restoreRate = GUI.HorizontalSlider(new Rect(75, 550, 200, 40), restoreRate, 0f, 0.2f);
//
//        GUI.Label(new Rect(300, 545, 200, 40), "Restore Rate:" + restoreRate);
//    }
}
