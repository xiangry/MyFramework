using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace component.UIAppend
{

    [LuaCallCSharp()]
    public class point_progress : MonoBehaviour
    {
        public Transform Target;

        public Vector2 FinalPosition;

        public float progress = 0.0f;

        void Awake()
        {
            if (Target)
            {
                FinalPosition = Target.GetComponent<RectTransform>().anchoredPosition;
            }

        }

        public void SetProgress(float num)
        {
            if (num >= 0 && num <= 1)
            {

                this.progress = num;
                SetPosition(num);
            }
        }

        public void SetPosition(float num)
        {
            if (FinalPosition != null)
            {
                Vector2 final = FinalPosition;
                var rt = gameObject.GetComponent<RectTransform>();
                rt.anchoredPosition = final*num;
            }
        }

    }
}
