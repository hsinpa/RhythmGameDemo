using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Snake;

namespace Hsinpa.Creator {
    public class SnakePathCreator : MonoBehaviour
    {
        [SerializeField]
        private SnakePath _snakePath;

        [SerializeField, HideInInspector]
        private bool _enableAutoCtrlPoint;
        public bool enableAutoContorlPoint {
            get { return _enableAutoCtrlPoint; }
            set
            {
                _enableAutoCtrlPoint = value;
            }
        }


        public SnakePath snakePath => _snakePath;

        public void CreateBasicPathSetup() {
            _snakePath = new SnakePath();

            //Start Point
            _snakePath.Push(Vector3.zero);

            //Start Ctrl Point
            _snakePath.Push(Vector3.zero + Vector3.up);

            Vector3 EndPoint = new Vector3(0, 0, 2);
            //End Ctrl Point
            _snakePath.Push(EndPoint + Vector3.down);

            //End  Point
            _snakePath.Push(EndPoint);


            _snakePath.AddSegment(new Vector3(0, 0, 3));
        }


    }
}