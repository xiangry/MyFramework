using System.Collections;
using System.Collections.Generic;
using Common.Utility;
using UnityEngine;
using UnityEngine.UI;

public class TestExtension : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform text_trs = transform.FindRec("AccountDesc");
        Text text = text_trs.GetComponent<Text>();
        text.text = "测试文字";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
