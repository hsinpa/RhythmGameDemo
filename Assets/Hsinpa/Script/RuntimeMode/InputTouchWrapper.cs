using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Hsinpa.InputSystem
{
    public class InputTouchWrapper
    {
        private TouchInfo[] touchInfoCaches;
        private int cacheCount = 4;

        public InputTouchWrapper() {
            EnhancedTouch.EnhancedTouchSupport.Enable();

            touchInfoCaches = new TouchInfo[cacheCount];

            for (int i = 0; i < cacheCount; i++) {
                touchInfoCaches[i] = new TouchInfo();
            }

            ResetTouchInfo();
        }


        public TouchInfo[] HasTouch() {
            ResetTouchInfo();


#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                touchInfoCaches[0].touchId = 0;
                touchInfoCaches[0].touchScreenPosition = Input.mousePosition;
            }
#else
            int touchCount = EnhancedTouch.Touch.activeFingers.Count;
            if (touchCount > 0)
            {
                int filterTouchCount = Mathf.Clamp(touchCount, 0, cacheCount);
                for (int i = 0; i < filterTouchCount; i++)
                {
                    var touch = EnhancedTouch.Touch.activeFingers[i];
                    touchInfoCaches[i].touchId = touch.index;
                    touchInfoCaches[i].touchScreenPosition = touch.screenPosition;
                }
            }
#endif
            return touchInfoCaches;
        }

        private void ResetTouchInfo() {
            for (int i = 0; i < cacheCount; i++)
            {
                touchInfoCaches[i].touchId = -1;
            }
        }

        public struct TouchInfo {
            public bool isValid => touchId >= 0;

            public int touchId;

            public Vector3 touchScreenPosition;
        }

    }
}