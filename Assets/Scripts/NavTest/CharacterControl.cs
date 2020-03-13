using System;
using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;
using UnityEngine.AI;

public class CharacterControl : MonoBehaviour {
	
    private CharacterController cc;
    private Animator animator;
    private ETCJoystick m_Joystick;

    public float speed = 3f;
    
    public bool isCamRotate = true;
    public Camera _mainCamera;
    public float rotateSpeed  = 1;
    public float maxRotSpeed1 = 4.0f;
    public float maxRotSpeed2 = -4.0f;
    private float moveX = 0;
    public Transform lookTarget;
    public Vector3 path;
    private float length = 0f;
    
    public bool playerMoveFlag { get; set; } // 限制Player寻路和摇杆同时生效
    void Awake()
    {
        _mainCamera = FindObjectOfType<Camera>();

        path = _mainCamera.transform.position - lookTarget.position;
        length = path.magnitude;
        
        EasyTouch.On_Swipe += EasyTouch_On_Swipe;
        
        playerMoveFlag = true;
        cc = gameObject.GetComponent<CharacterController>();
        cc.center = new Vector3(0,1,0);
//        animator = gameObject.GetComponent<Animator>();
//        animator.CrossFade("worldrun",0);
        m_Joystick = FindObjectOfType<ETCJoystick>();
        m_Joystick.onTouchStart.AddListener(JsOnTouchStart);
        m_Joystick.onMoveSpeed.AddListener(JsOnMoveSpeed);
    }

    // Wait end of frame to manage charactercontroller, because gravity is managed by virtual controlleras
    void LateUpdate()
    {
        _mainCamera.transform.position = lookTarget.position + path;
        
        if (playerMoveFlag)
        {
            if (cc.isGrounded && (ETCInput.GetAxis("Vertical")!=0)){
                Logger.Log(string.Format("soldierRun ==----------== {0}", ETCInput.GetAxis("Vertical")));
                float runSpeed = ETCInput.GetAxis("Vertical");
                if (runSpeed > -0.1 && runSpeed < 0.1)
                {
                    runSpeed = 0.1f;
                }
                if (runSpeed < -0.1 && runSpeed >-0.5)
                {
                    runSpeed = Mathf.Abs(ETCInput.GetAxis("Vertical"));
                }
                if (runSpeed <-0.5)
                {
                    runSpeed = 0.5f;
                } 
//                animator.SetFloat("speed", runSpeed);
            
            }

            if (cc.isGrounded && ETCInput.GetAxis("Vertical")==0 && ETCInput.GetAxis("Horizontal")==0){
                Logger.Log("soldierIdleRelaxed======---------------");
//                animator.SetFloat("speed", 0);
            }

            if (!cc.isGrounded){
//                Logger.Log("soldierFalling======---------------");
                //human.CrossFade("soldierFalling");
            }

            if (cc.isGrounded && ETCInput.GetAxis("Vertical")==0 && ETCInput.GetAxis("Horizontal")>0){
                Logger.Log("soldierSpinRight======---------------");
                //human.CrossFade("soldierSpinRight");
            }

            if (cc.isGrounded && ETCInput.GetAxis("Vertical")==0 && ETCInput.GetAxis("Horizontal")<0){
                Logger.Log("soldierSpinLeft======---------------");
                //human.CrossFade("soldierSpinLeft");
            }
        }
    }

    private void JsOnTouchStart()
    {
        playerMoveFlag = true;
        gameObject.GetComponent<NavMeshAgent>().isStopped = true;
//        animator.SetFloat("speed", 0);
    }

    private void JsOnMoveSpeed(Vector2 arg0)
    {


        float x = arg0.x;
        float z = arg0.y;

        if (x != 0 || z != 0)
        {
            Vector3 targetDirection = new Vector3(x, 0, z);

            float y = Camera.main.transform.rotation.eulerAngles.y;

            targetDirection = Quaternion.Euler(0, y, 0) * targetDirection;


            transform.LookAt(targetDirection + transform.position);


            transform.Translate(targetDirection * Time.deltaTime * speed, Space.World);



        }
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
        _mainCamera.transform.position = lookTarget.position + path;
        //控制相机绕中心点垂直旋转(！注意此处的旋转轴时相机自身的x轴正方向！)
        _mainCamera.transform.RotateAround(lookTarget.position, Vector3.up, _mouseX * rotateSpeed);
        var look = _mainCamera.transform.position - lookTarget.position;
        path = look.normalized * length;
        _mainCamera.transform.position = lookTarget.position + path;
    }
    
    private void OnDestroy()
    {
        EasyTouch.On_Swipe -= EasyTouch_On_Swipe;
    }

}