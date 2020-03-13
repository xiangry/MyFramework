using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using XLua;


public class ToggleAppend : MonoBehaviour
{
    [SerializeField]
    public Toggle ListenToggle;
    [SerializeField]
    public List<GameObject> ActiveShow = new List<GameObject>();   //激活时展示
    [SerializeField]
    public List<GameObject> DeActiveShow = new List<GameObject>(); //不激活时展示
    void Start()
    {
        if (ListenToggle == null)
        {
            ListenToggle = gameObject.GetComponent<Toggle>();
        }
        ListenToggle.onValueChanged.AddListener(OnToggleChanged);
        
        ActiveShow.ForEach(item =>
        {
            item.SetActive(ListenToggle.isOn);
        });
            
        DeActiveShow.ForEach(item =>
        {
            item.SetActive(!ListenToggle.isOn);
        });
        
    }

    void OnToggleChanged(bool value)
    {
      
            ActiveShow.ForEach(item =>
            {
                item.SetActive(value);
            });
            
            DeActiveShow.ForEach(item =>
            {
                item.SetActive(!value);
            });
    }
    

    void OnDestroy()
    {
        ListenToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}

#if UNITY_EDITOR
public static class ToggleAppendExporter
{
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
        typeof(Toggle.ToggleEvent),
        typeof(UnityAction<bool>),
    };
}
#endif
//Toggle.ToggleEvent