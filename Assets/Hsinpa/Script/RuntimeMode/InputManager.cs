using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.InputSystem {
    public class InputManager : MonoBehaviour
    {

        [SerializeField]
        private SnakePathViewer snakePathViewer;

        Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {

            if (Input.GetMouseButton(0)) {
                OnScreenTouch(Input.mousePosition);
            }

        }

        private void OnScreenTouch(Vector2 screenPos) {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            snakePathViewer.OnMouseClick(ray);

            //Debug.Log($"Screen {screenPos}, Origin {ray.origin}, Direction {ray.direction}");
        }

    }
}
