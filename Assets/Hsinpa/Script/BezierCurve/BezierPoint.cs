using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Bezier {
    public class BezierPoint : MonoBehaviour
    {

        [SerializeField]
        private BezierCtrlPoint _BezierCtrlPoint;

        public Vector3 BezierCtrlPoint => _BezierCtrlPoint.transform.position;

    }
}
