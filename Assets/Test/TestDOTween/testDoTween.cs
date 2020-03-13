using System.Collections;
using System.Collections.Generic;
using Common.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class testDoTween : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Image image = transform.GetComponent<Image>();
        Canvas canvas = transform.GetComponent<Canvas>();
//        image.transform.SetLocalScale(5,5,1);
        //DoMove的坐标系是左下角为准,移动到100,100位置
//        image.rectTransform.DOMove (new Vector3(100,100,0),1f);
        //以目前坐标点向移动到当前坐标x+100,当前坐标y+100
        image.rectTransform.DOLocalMove(new Vector2(image.rectTransform.sizeDelta.x + 100,image.rectTransform.localPosition.y + 100),1f);
        //当前sacle(1,1,1)1秒内添加到(3,3,1)
//        image.rectTransform.DOBlendableScaleBy (new Vector2(2,2),1f);
        //旋转到180度
//        image.rectTransform.DORotate (new Vector3(0,0,180),1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
