using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour
{
//    public Transform target;
//    public float damping = 6.0f;
//    public bool smooth = true;
//    
//    // Start is called before the first frame update
//    void Start()
//    {
//        if (GetComponent<Rigidbody>() != null)
//        {
//            GetComponent<Rigidbody>().freezeRotation = true;
//        }
//    }
//
//    private void LateUpdate()
//    {
//        if (target != null)
//        {
//            if (smooth)
//            {
//                var rotation = Quaternion.LookRotation((target.position - transform.position));
//                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
//            }
//            else
//            {
//                transform.LookAt(target);
//                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
//            }
//        }
//    }
}
