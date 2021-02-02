using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Utility {
    public class SnakeUtility
    {

        public static Vector3 BezierCurve(Vector3 StartPoint, Vector3 StartCtrlPoint, Vector3 EndCtrlPoint, Vector3 EndPoint, float t)
        {

            Vector3 PS = Vector3.Lerp(StartPoint, StartCtrlPoint, t);
            Vector3 PE = Vector3.Lerp(EndCtrlPoint, EndPoint, t);

            Vector3 CC = Vector3.Lerp(StartCtrlPoint, EndCtrlPoint, t);

            Vector3 SC = Vector3.Lerp(PS, CC, t);
            Vector3 EC = Vector3.Lerp(CC, PE, t);

            return Vector3.Lerp(SC, EC, t);
        }
    }
}
