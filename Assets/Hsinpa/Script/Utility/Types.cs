using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa {
    public class Types
    {

        public struct GenenralMeshType
        {


        }

        [System.Serializable]
        public struct BezierSegmentInfo
        {
            public int SegmentIndex;
            public float Interval;
            public Vector3 Position;
        }

        public enum SnakePathType {
            Point, ControlPoint
        }
    }
}
