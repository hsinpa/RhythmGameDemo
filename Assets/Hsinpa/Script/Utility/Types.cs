using JetBrains.Annotations;
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
            public Vector3 Position;
        }

        public struct BezierSegmentData {
            public int SliceCount;
            public int SegmentIndex;
            public List<Vector3> vectors;
        }

        public struct BezierSegmentType
        {
            public BezierSegmentData bezierSegmentData;

            //Won't be saved
            public List<Vector3> vectors;
        }

        public struct MeshInfo {
            public List<Vector3> vertices;
            public List<int> triangles;
            public List<Vector2> uv;
        }

        [System.Serializable]
        public struct LevelJSON {
            public string bpm;

            public LevelComponent[] sequence;
        }

        [System.Serializable]
        public struct LevelComponent {
            public string type;
            public float time;
            public string value;
        }

        public enum SnakePathType {
            Point, ControlPoint
        }
    }
}
