using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.InputSystem {
    public class InputManager : MonoBehaviour
    {

        [SerializeField]
        private SnakePathViewer snakePathViewer;

        [SerializeField]
        private MeshRenderer[] touchIndicators;

        Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            foreach (MeshRenderer meshRenderer in touchIndicators)
            {
                meshRenderer.enabled = false;
            }

            if (Input.GetMouseButton(0)) {
                OnScreenTouch(Input.mousePosition);
            }
        }

        private void OnScreenTouch(Vector2 screenPos) {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            var onClickResult =  snakePathViewer.OnMouseClick(ray);

            if (onClickResult.isValid) {
                MeshRenderer tIndicator = GetAvailableTouchIndicator();

                if (tIndicator != null) {
                    tIndicator.enabled = true;
                    tIndicator.transform.position = onClickResult.touchPoint;
                }
            }

            //Debug.Log($"Screen {screenPos}, Origin {ray.origin}, Direction {ray.direction}");
        }

        private MeshRenderer GetAvailableTouchIndicator() {
            foreach (MeshRenderer meshRenderer in touchIndicators) {
                if (!meshRenderer.enabled)
                    return meshRenderer;
            }

            return null;
        }


    }
}
