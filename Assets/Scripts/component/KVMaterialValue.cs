using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class KVMaterialValue : MonoBehaviour
{
    public string valueKey = "_appear";
    public float value = 0f;
    private float lastValue = 0f;

    
    public bool useDefault = false;
    public float defaultValue = 0f;

    private Material sMaterial;

    private void Awake()
    {
        sMaterial = gameObject.GetComponent<RawImage>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastValue != value)
        {
            lastValue = value;
            sMaterial.SetFloat(valueKey, value);
        }
    }

    private void OnEnable()
    {
        if (useDefault)
        {
            value = defaultValue;
        }
        lastValue = value;
        sMaterial.SetFloat(valueKey, value);
    }
}
