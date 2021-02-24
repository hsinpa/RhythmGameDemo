using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hsinpa.InputSystem {
    public class InputManager : MonoBehaviour
    {

        [SerializeField]
        private SnakePathViewer snakePathViewer;

        [SerializeField]
        private InputTouchPoint[] TouchPointArray;

        private float indicatorReserveTime = 0.1f;
        private InputTouchWrapper _inputTouchWrapper;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            _inputTouchWrapper = new InputTouchWrapper();
        }

        private void Update()
        {
            foreach (InputTouchPoint touchPoint in TouchPointArray)
            {
                if (touchPoint.lastEnableTime < Time.time) {
                    touchPoint.meshRenderer.enabled = false;
                    touchPoint.touchID = -1;
                }
            }

            InputTouchWrapper.TouchInfo[] touchInfoArray = _inputTouchWrapper.HasTouch();
            int touchCount = touchInfoArray.Length;
            for (int i = 0; i < touchCount; i++) {
                if (touchInfoArray[i].isValid)
                    OnScreenTouch(touchInfoArray[i].touchScreenPosition, touchInfoArray[i].touchId);
            }
        }

        private void OnScreenTouch(Vector2 screenPos, int touchID) {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            var onClickResult = snakePathViewer.OnMouseClick(ray);

            if (onClickResult.isValid) {
                InputTouchPoint tIndicator = GetAvailableTouchIndicator(touchID: touchID);

                if (tIndicator != null) {
                    tIndicator.meshRenderer.enabled = true;

                    if (tIndicator.touchID == -1)
                        tIndicator.transform.position = onClickResult.touchPoint;
                    else
                        tIndicator.transform.position = Vector3.Lerp(tIndicator.transform.position, onClickResult.touchPoint, 0.2f);

                    tIndicator.touchID = touchID;

                    tIndicator.lastEnableTime = Time.time + indicatorReserveTime;
                }
            }

            //Debug.Log($"Screen {screenPos}, Origin {ray.origin}, Direction {ray.direction}");
        }

        private InputTouchPoint GetAvailableTouchIndicator(int touchID) {
            foreach (InputTouchPoint touchPoint in TouchPointArray) {

                if (touchPoint.touchID == touchID && touchPoint.meshRenderer.enabled) {
                    return touchPoint;
                }

                if (!touchPoint.meshRenderer.enabled)
                    return touchPoint;
            }

            return null;
        }


    }
}
