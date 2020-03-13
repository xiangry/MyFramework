using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Nav3 : MonoBehaviour
{
    public int TriggerId; // 触发事件id
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        Logger.Log(string.Format("TriggerIdTriggerId=={0}=---",TriggerId));
        Logger.Log(string.Format("OnCollisionEnter=={0}=---",collider));
    }
}
