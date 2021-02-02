using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Bezier
{
    public class BezierCurve : MonoBehaviour
    {
        [SerializeField]
        private BezierPoint StartPoint;

        [SerializeField]
        private BezierPoint EndPoint;


        [SerializeField, Range(1, 10)]
        private int Segment;

        private void Start()
        {

            List<Vector3> points = new List<Vector3>(Segment);

            for (int i = 0; i < Segment; i++) {

                float t = (i / (float)Segment);
                Vector3 PS = Vector3.Lerp(StartPoint.transform.position, StartPoint.BezierCtrlPoint, t);
                Vector3 PE = Vector3.Lerp(EndPoint.BezierCtrlPoint, EndPoint.transform.position, t);

                Vector3 CC = Vector3.Lerp(StartPoint.BezierCtrlPoint, EndPoint.BezierCtrlPoint, t);

                Vector3 SC = Vector3.Lerp(PS, CC, t);
                Vector3 EC = Vector3.Lerp(CC, PE, t);


                Vector3 AP = Vector3.Lerp(SC, EC, t);

                points.Add(AP);

            }

            //Debug.DrawLine(points[Segment-1], EndPoint.transform.position, Color.white, 100);

        }

    }
}