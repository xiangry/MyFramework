using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Playables;

public class TestTimeLine : MonoBehaviour
{
    public GameObject caster = null;

    public GameObject target1 = null;
    public GameObject target2 = null;
    public GameObject target3 = null;
    public GameObject target4 = null;
    public GameObject target5 = null;

    public GameObject camera = null;

    public float timeScale = 0.001f;
    public float curTime = 0.0f;

    private const string BindParentCaster1 = "skillcaster/char1";
    private const string BindParentCaster2 = "skillcaster/char2";
    private const string BindParentCaster3 = "skillcaster/char3";
    private const string BindParentCaster4 = "skillcaster/char4";
    private const string BindParentCaster5 = "skillcaster/char5";
    
    private const string BindParentTarget1 = "skilltarget/char1";
    private const string BindParentTarget2 = "skilltarget/char2";
    private const string BindParentTarget3 = "skilltarget/char3";
    private const string BindParentTarget4 = "skilltarget/char4";
    private const string BindParentTarget5 = "skilltarget/char5";
    
    private const string BindParentCamera = "cameragroup";

    public PlayableDirector director = null;

    private readonly Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();

    // Start is called before the first frame update
    void Start()
    {

        director.timeUpdateMode = DirectorUpdateMode.GameTime;
        
        Debug.Log($"time line duration {director.duration}");
        
        foreach (var bind in director.playableAsset.outputs)
        {
            if (!bindingDict.ContainsKey(bind.streamName))
            {
                bindingDict.Add(bind.streamName, bind);
            }
        }

        caster.GetComponent<Animator>().speed = 0;
        
        target1.GetComponent<Animator>().speed = 0;
        target2.GetComponent<Animator>().speed = 0;
        target3.GetComponent<Animator>().speed = 0;
        target4.GetComponent<Animator>().speed = 0;
        target5.GetComponent<Animator>().speed = 0;
        

        SetTrackDynamic("skillcaster_char1", caster);
        SetTrackDynamic("skilltarget_char1", target1);
        SetTrackDynamic("skilltarget_char2", target2);
        SetTrackDynamic("skilltarget_char3", target3);
        SetTrackDynamic("skilltarget_char4", target4);
        SetTrackDynamic("skilltarget_char5", target5);
        
        
        SetBindObjPos(BindParentCaster1, caster);
        SetBindObjPos(BindParentTarget1, target1);
        SetBindObjPos(BindParentTarget2, target2);
        SetBindObjPos(BindParentTarget3, target3);
        SetBindObjPos(BindParentTarget4, target4);
        SetBindObjPos(BindParentTarget5, target5);
        SetBindObjPos(BindParentCamera, camera);
        
        
    }

    /// <summary>
    /// 动态设置轨道
    /// </summary>
    /// <param name="trackName"></param>
    /// <param name="gameObject"></param>
    public void SetTrackDynamic(string trackName, GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }
        if (gameObject.GetComponent<Animator>() == null)
        {
            Logger.LogError($"PlayableDirector 绑定的对象({trackName})没有Animator组件");
            return;
        }
        if (bindingDict.TryGetValue(trackName, out PlayableBinding pb))
        {
            director.SetGenericBinding(pb.sourceObject, gameObject);
        }
    }

    public void SetBindObjPos(string name, GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }        
        
        GameObject parent = GameObject.Find(director.name + "/" + name);
        if (parent != null)
        {
            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            Logger.LogError($"绑定对象{name}父节点找不到");
        }
    }

    private void Update()
    {
        curTime += timeScale * Time.deltaTime;
        if (curTime > director.duration)
        {
            curTime = 0;
        }
        director.time = curTime;
    }
}