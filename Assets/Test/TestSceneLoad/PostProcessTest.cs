using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessTest : MonoBehaviour
{
    PostProcessLayer postLayer;
    PostProcessVolume postVolume;

    public Camera cam;
    public PostProcessProfile profile;
    // Start is called before the first frame update
    void Awake()
    {
        this.gameObject.AddComponent<PostProcessLayer>();
        this.gameObject.AddComponent<PostProcessVolume>();

        postLayer = this.gameObject.GetComponent<PostProcessLayer>();
        postVolume = this.gameObject.GetComponent<PostProcessVolume>();

        postLayer.volumeTrigger = this.gameObject.transform;
        postLayer.volumeLayer = LayerMask.GetMask("PostProcessing");

        postVolume.isGlobal = true;
        postVolume.profile = profile;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
