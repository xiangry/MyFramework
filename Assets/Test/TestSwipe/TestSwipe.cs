using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;

public class TestSwipe : MonoBehaviour
{
    private bool isSwipe = false;
    private EasyTouch.SwipeDirection lastSwipe = EasyTouch.SwipeDirection.None;
    
    // Start is called before the first frame update
    void Start()
    {
        EasyTouch.On_SwipeStart += OnSwipeStart;
        EasyTouch.On_Swipe += OnSwipe;
        EasyTouch.On_SwipeEnd += OnSwipeEnd;
    }

    void OnSwipeStart(Gesture gesture)
    {
        Debug.Log($"OnSwipeStart swipe:{gesture.swipe} Length:{gesture.swipeLength} swipeVector:{gesture.swipeVector} twistAngle:{gesture.twistAngle}");
    }

    void OnSwipe(Gesture gesture)
    {
        isSwipe = true;
        if (gesture.swipe == EasyTouch.SwipeDirection.Other)
        {
            return;
        }

        if (gesture.swipe != lastSwipe)
        {
            Debug.Log($"滑动方向 {gesture.swipe}");
            lastSwipe = gesture.swipe;
        }
    }
    
    void OnSwipeEnd(Gesture gesture)
    {
        lastSwipe = EasyTouch.SwipeDirection.None;
        isSwipe = false;
        Debug.Log($"OnSwipeEnd swipe:{gesture.swipe} Length:{gesture.swipeLength} swipeVector:{gesture.swipeVector} twistAngle:{gesture.twistAngle}");
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
