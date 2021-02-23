using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Snake;
using UnityEngine.Rendering.UI;

namespace Hsinpa.Creator {
    [ExecuteInEditMode]
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

                if (_enableAutoCtrlPoint)
                    SmoothCtrlPoints(0, snakePath.PointCount);
            }
        }

        public SnakePath snakePath => _snakePath;

        public SnakeMesh snakeMeshPrefab;

        [SerializeField]
        private SnakeMesh _snakeMesh;

        public Vector2 XAxisConstraints;

        public Vector2 YAxisConstraints;

        private void Start()
        {
            RenderPathLayoutToMesh();
        }

        public void CreateBasicPathSetup() {
            // _snakePath = new SnakePath();

            _snakePath.Reset();

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

        public void SmoothCtrlPoints(int startIndex, int endIndex) {

            for (int i = startIndex; i <= endIndex; i += 3 )
            {
                if (i <= 0 || i >= snakePath.PointCount - 1 || !SnakePath.IsAnchorPoint(i)) continue;

                Vector3 anchorFaceDir = (snakePath[i + 3] - snakePath[i - 3]).normalized;
                float halfPreviousDist = Vector3.Distance(snakePath[i - 3], snakePath[i]) * 0.5f;
                float halfForwardDist = Vector3.Distance(snakePath[i + 3], snakePath[i]) * 0.5f;

                //Debug.Log($"index {i},halfPreviousDist {halfPreviousDist}, halfForwardDist {halfForwardDist}");

                snakePath.ForceUpdate(snakePath[i] - (anchorFaceDir * halfPreviousDist), (i - 1));
                snakePath.ForceUpdate(snakePath[i] + (anchorFaceDir * halfForwardDist), (i + 1));
            }
        }

        public void RenderPathLayoutToMesh() {
            if (_snakeMesh == null) return;
            
            _snakeMesh.SetUp();

            _snakeMesh.SetSnakePath(snakePath, true);

            _snakeMesh.RenderMesh();
        }

        public void SetSnakePath(SnakePath snakePath) {
            if (_snakeMesh == null) return;
            _snakePath = snakePath;

            RenderPathLayoutToMesh();
        }

    }
}