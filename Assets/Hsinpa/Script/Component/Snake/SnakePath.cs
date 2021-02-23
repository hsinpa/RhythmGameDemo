using System.Collections;
using System.Collections.Generic;
using Hsinpa.Utility;
using UnityEngine;

namespace Hsinpa.Snake {

    [System.Serializable, CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Path", order = 2)]
    public class SnakePath : ScriptableObject
    {
        private Types.BezierSegmentInfo _bezierSegmentInfo = new Types.BezierSegmentInfo();

        private const float bezierStep = 0.05f;
        private float bezierDist = 0.1f;

        private List<Types.BezierSegmentInfo> _cacheSegmentInfoArray = new List<Types.BezierSegmentInfo>();

        public Types.SnakeTag tag;        

        [SerializeField]
        private List<Vector3> Points = new List<Vector3>();

        public Vector2 OffsetPosition = Vector2.zero;

        public Vector3 this[int i]
        {
            get
            {
                return Points[i];
            }
        }

        public int PointCount => Points.Count;

        public int NumSegments
        {
            get
            {
                //The default 4 points count as one
                return (Points.Count - 4) / 3 + 1;
            }
        }

        public static bool IsAnchorPoint(int index) => (index % 3 == 0);

        public void AddSegment(Vector3 anchorPos)
        {
            //Add Ctrl to the previous END POINT
            Points.Add(Points[Points.Count - 1] * 2 - Points[Points.Count - 2]);

            //Current Point's Ctrl Point
            Points.Add((Points[Points.Count - 1] + anchorPos) * .5f);

            Points.Add(anchorPos);
        }

        public Vector3[] GetPointsInSegment(int i)
        {
            //Sequence Order => StartP, SC, EC, EndPoint
            return new Vector3[] { Points[i * 3], Points[i * 3 + 1], Points[i * 3 + 2], Points[i * 3 + 3] };
        }

        public void UpdateAnchor(Vector3 newPosition, int i) {

            //Debug.Log(i % 3);

            Vector3 deltaMove = newPosition - Points[i];
            Points[i] = newPosition;


            //If Anchor point is selected;
            if (i % 3 == 0)
            {
                //Move Control point together
                if (i + 1 < PointCount)
                {
                    Points[i + 1] += deltaMove;
                }
                if (i - 1 >= 0)
                {
                    Points[i - 1] += deltaMove;
                }
            }

            //If Control Point is selected

            else
            {
                //Boolean, Check next point is anchor
                bool nextPointIsAnchor = (i + 1) % 3 == 0;
                //If yes, then the corresponding Control point is 2 index away or vice versa
                int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
                //Get Anchor Point Index
                int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

                if (correspondingControlIndex >= 0 && correspondingControlIndex < PointCount)
                {
                    float dst = (Points[anchorIndex] - Points[correspondingControlIndex]).magnitude;
                    Vector3 dir = (Points[anchorIndex] - newPosition).normalized;

                    //Reverse
                    Points[correspondingControlIndex] = Points[anchorIndex] + dir * dst;
                }
            }
        }

        public void ForceUpdate(Vector3 position, int index) {
            Points[index] = position;
        }

        public void Insert(Vector3 position, int index) {
            Points.Insert(index, position);
        }

        public void SplitSegment(Vector3 newAnchorPos, int segmentIndex) {
            //Control anchor need re position
            Points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, newAnchorPos, Vector3.zero });
        }

        public void Push(Vector3 position) {
            Points.Add(position);
        }

        public void Delete(int anchorIndex) {
            if (!IsAnchorPoint(anchorIndex)) return;

            //Minimum segment count
            if (NumSegments > 1) {

                //Start Anchor
                if (anchorIndex == 0)
                {
                    Points.RemoveRange(0, 3);
                }
                //End Anchor
                else if (anchorIndex == PointCount - 1)
                {
                    Points.RemoveRange(anchorIndex - 2, 3);
                }
                else
                {
                    //The rest anchor
                    Points.RemoveRange(anchorIndex - 1, 3);
                }
            }
        }

        public List<Types.BezierSegmentInfo> GetSegmentBezierSteps(int segmentIndex) {
            _cacheSegmentInfoArray.Clear();

            Vector3[] points = GetPointsInSegment(segmentIndex);

            for (float t = 0f; t <= 1; t += bezierStep) {
                Vector3 bezierCurveDot = SnakeUtility.BezierCurve(points[0], points[1], points[2], points[3], t);

                //float tempInterval = t;
                //float dist = 100;

                //if (t > 0) {

                //    while (dist < bezierDist)
                //    {
                //        tempInterval *= 0.5f;
                //        Vector3 newBezierCurveDot = SnakeUtility.BezierCurve(points[0], points[1], points[2], points[3], tempInterval);

                //        dist = Vector3.Distance(bezierCurveDot, newBezierCurveDot);
                //    }
                //}

                _bezierSegmentInfo.SegmentIndex = segmentIndex;
                _bezierSegmentInfo.Position = bezierCurveDot;

                _cacheSegmentInfoArray.Add(_bezierSegmentInfo);
            }

            return _cacheSegmentInfoArray;
        }

        public void Reset() {
            Points.Clear();
        }

    }
}