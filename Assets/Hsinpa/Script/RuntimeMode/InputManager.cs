using System.Collections;
using System.Collections.Generic;
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

        Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
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

            if (Input.GetMouseButton(0)) {
                OnScreenTouch(Input.mousePosition);
            }
        }

        private void OnScreenTouch(Vector2 screenPos) {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            var onClickResult = snakePathViewer.OnMouseClick(ray);

            if (onClickResult.isValid) {
                InputTouchPoint tIndicator = GetAvailableTouchIndicator(touchID : 0);

                if (tIndicator != null) {
                    tIndicator.meshRenderer.enabled = true;

                    if (tIndicator.touchID == -1)
                        tIndicator.transform.position = onClickResult.touchPoint;
                    else
                        tIndicator.transform.position = Vector3.Lerp(tIndicator.transform.position, onClickResult.touchPoint, 0.2f);

                    tIndicator.touchID = 0;

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
