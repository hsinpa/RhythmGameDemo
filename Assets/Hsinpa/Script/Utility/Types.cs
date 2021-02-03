using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa {
    public class Types
    {

        [System.Serializable]
        public struct BezierSegmentInfo
        {
            public int SegmentIndex;
            public float Interval;
            public Vector3 Position;
        }

        public struct MeshInfo {
            public List<Vector3> vertices;
            public List<int> triangles;
            public List<Vector2> uv;
        }

        public enum SnakePathType {
            Point, ControlPoint
        }
    }
}
