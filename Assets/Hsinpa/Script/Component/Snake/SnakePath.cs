﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Snake {

    [System.Serializable]
    public class SnakePath
    {
        [SerializeField]
        private List<Vector3> Points = new List<Vector3>();

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

        public void Update(Vector3 newPosition, int i) {

            Debug.Log(i % 3);

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

        public void Insert(Vector3 position, int index) {
            Points.Insert(index, position);
        }

        public void Push(Vector3 position) {
            Points.Add(position);
        }



    }
}