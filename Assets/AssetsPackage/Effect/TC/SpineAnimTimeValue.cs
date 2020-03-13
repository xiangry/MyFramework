using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SpineAnimTimeValue : MonoBehaviour
{
    public float time = 0f;

    private Material sMaterial;

    // Start is called before the first frame update
    void Start()
    {
        sMaterial = gameObject.GetComponent<SkeletonGraphic>().material;
    }

    // Update is called once per frame
    void Update()
    {
        sMaterial.SetFloat("_Float0", time);
    }

    private void OnEnable()
    {
//        time = defaultTime;
        sMaterial.SetFloat("_Float0", time);
    }
}
