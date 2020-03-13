using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJoyControl : MonoBehaviour
{
    private ETCJoystick joystick;
    private CharacterController _characterControl;

    public Vector3 cameraPos;
    public Vector3 cameraRotation;

    public float speed = 50f;

    public Camera camera;

    
    // Start is called before the first frame update
    void Start()
    {
        _characterControl = GetComponent<CharacterController>();
        joystick = GameObject.Find("EasyTouchControlsCanvas/Joystick").GetComponent<ETCJoystick>();

        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cameraPos = camera.transform.position;
        cameraRotation = camera.transform.eulerAngles;
        
        joystick.onMoveSpeed.AddListener((v) =>
        {
            if (v.magnitude != 0)
            {
                Vector3 direction = new Vector3(v.x, 0, v.y);
                var y = Camera.main.transform.rotation.eulerAngles.y;
                var targetDirection  = Quaternion.Euler(0, y, 0) * direction;
                transform.LookAt(targetDirection  + transform.position);
                transform.Translate(targetDirection  * Time.deltaTime * speed, Space.World);
//                _characterControl.Move(direction * speed * Time.deltaTime); //利用 CharacterController 驱动Transform
//                _characterControl.Move(direction * speed * Time.deltaTime); //利用 CharacterController 驱动Transform
//                transform.rotation = Quaternion.LookRotation(direction, cam);  //这一句控制旋转，依旧不变
//                transform.rotation = Quaternion.Euler(ctv);  //这一句控制旋转，依旧不变
            }
            // 存储坐标系的偏移
//            Debug.Log($"-----onMoveSpeed({v})");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
