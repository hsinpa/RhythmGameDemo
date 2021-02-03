using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hsinpa.Snake;
using Hsinpa.Utility;
using UnityEngine.SceneManagement;

namespace Hsinpa.Creator
{
    [CustomEditor(typeof(SnakePathCreator))]
    public class SnakePathEditor : Editor
    {
        SnakePathCreator creator;
        Tool LastTool = Tool.None;

        Vector3 _snap = Vector3.one * 0.5f;
        float minBezierDistThreshold = 0.2f;
        //When minBezierDistThreshold fail, use this to check lastBezierSegmentInfo
        float minRecordBezierDistThreshold = 1f;

        BezierSegmentInfo lastBezierSegmentInfo;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SnakePathCreator myScript = (SnakePathCreator)target;
            if (GUILayout.Button("Default Snake Layout"))
            {
                Undo.RecordObject(creator, "Create Default Layout");

                myScript.CreateBasicPathSetup();
                SceneView.RepaintAll();
            }

            bool isEnableAutoCtrlP = GUILayout.Toggle(creator.enableAutoContorlPoint, "Enable Auto Ctrl Point");
            if (isEnableAutoCtrlP != creator.enableAutoContorlPoint) {
                Undo.RecordObject(creator, "Enable Auto Ctrl Point");
                creator.enableAutoContorlPoint = isEnableAutoCtrlP;
            }
        }

        void OnSceneGUI()
        {
            Event guiEvent = Event.current;

            Input(guiEvent);
            Draw(guiEvent);
        }

        void Input(Event guiEvent)
        {
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
            Vector3 direction = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).direction;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                Vector3 lastPoint = creator.snakePath[creator.snakePath.PointCount - 1];
                float magnitude = Vector3.Distance(lastPoint, mousePos);

                Vector3 finalPoint = mousePos + (direction * magnitude);

                Undo.RecordObject(creator, "Add segment");
                creator.snakePath.AddSegment(finalPoint);

                AutoSetControlPointIfEnable(creator.snakePath.PointCount - 1);
            }

            //Delete Anchor
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1) {

                int removeIndex = FindTheMostClosetIndex(mousePos, direction);

                if (removeIndex >= 0) {

                    Undo.RecordObject(creator, "Delete segment");

                    Debug.Log("FindTheMostClosetIndex " + removeIndex);

                    creator.snakePath.Delete(removeIndex);
                    AutoSetControlPointIfEnable(removeIndex);
                }
            }

            if (guiEvent.type == EventType.MouseMove && guiEvent.control)
            {
                lastBezierSegmentInfo = FindClosestSegment(mousePos, direction);

                if (lastBezierSegmentInfo.SegmentIndex >= 0)
                {
                    //Debug.Log($"SegmentIndex {lastBezierSegmentInfo.SegmentIndex}, t {lastBezierSegmentInfo.t}, dist {lastBezierSegmentInfo.dist}");
                    SceneView.RepaintAll();
                }

            }


            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control) {
                if (lastBezierSegmentInfo.SegmentIndex >= 0)
                {
                    creator.snakePath.SplitSegment(lastBezierSegmentInfo.position, lastBezierSegmentInfo.SegmentIndex);

                    int anchorIndex = lastBezierSegmentInfo.SegmentIndex * 3;
                    creator.SmoothCtrlPoints(anchorIndex -3, anchorIndex+3);
                    SceneView.RepaintAll();
                }
            }

            if (!guiEvent.control)
                lastBezierSegmentInfo.SegmentIndex = -1;
        }

        void Draw(Event guiEvent) {

            if (creator.snakePath != null) {

                for (int i = 0; i < creator.snakePath.NumSegments; i++)
                {
                    Vector3[] points = creator.snakePath.GetPointsInSegment(i);
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);

                    Color bezierColor = (i == lastBezierSegmentInfo.SegmentIndex) ? Color.yellow : Color.green;

                    if (i == lastBezierSegmentInfo.SegmentIndex) {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, lastBezierSegmentInfo.position, Quaternion.identity, 0.2f, EventType.Repaint);
                    }

                    Handles.DrawBezier(points[0], points[3], points[1], points[2], bezierColor, null, 2);
                }


                for (int i = 0; i < creator.snakePath.PointCount; i++) {
                    bool isAnchorP = SnakePath.IsAnchorPoint(i);
                    Handles.color = (isAnchorP) ? Color.red : Color.blue;

                    Vector3 newPos = Handles.FreeMoveHandle(creator.snakePath[i], Quaternion.identity, .2f, _snap, Handles.CylinderHandleCap);

                    //Move Anchor Around
                    if (creator.snakePath[i] != newPos && (!creator.enableAutoContorlPoint || ( (creator.enableAutoContorlPoint && isAnchorP) ||
                        //The first / last control point leave to player to decide
                        i <= 1 || i >= creator.snakePath.PointCount - 2) ))
                    {
                        Undo.RecordObject(creator, "Move point");

                        creator.snakePath.Update(newPos, i);

                        if (creator.enableAutoContorlPoint) {
                            AutoSetControlPointIfEnable(i);
                        }
                    }
                }
            }
        }

        int FindTheMostClosetIndex(Vector3 mousePos, Vector3 mouseDir) {
            int clostetIndex = -1;
            float clostestDir = 0;

            for (int i = 0; i < creator.snakePath.PointCount; i += 3)
            {
                Vector3 targetToCamDir = (creator.snakePath[i] - mousePos).normalized;
                float dotValue = Vector3.Dot(targetToCamDir, mouseDir);

                //Exist early
                if (dotValue < clostestDir) return clostetIndex;

                if (dotValue > 0.999f)
                {
                    clostestDir = dotValue;

                    clostetIndex = i;
                    //Debug.Log("Find index " + i + ", " + dotValue);
                }
            }

            return clostetIndex;
        }

        BezierSegmentInfo FindClosestSegment(Vector3 mousePos, Vector3 mouseDir) {
            Vector3 recordBezierCurveDot = Vector3.zero;

            for (int i = 0; i < creator.snakePath.NumSegments; i++)
            {
                Vector3[] points = creator.snakePath.GetPointsInSegment(i);

                float dist = 1000;
                float record_t = 0;

                for (float t = 0.1f; t < 1; t += 0.1f)
                {
                    Vector3 bezierCurveDot = SnakeUtility.BezierCurve(points[0], points[1], points[2], points[3], t);
                    float distToCamera = (mousePos - bezierCurveDot).magnitude;
                    Vector3 simulateMousePoint = mousePos + (distToCamera * mouseDir);

                    Debug.Log(simulateMousePoint);

                    float tempDist = HandleUtility.DistancePointBezier(simulateMousePoint, points[0], points[3], points[1], points[2]);

                    if (tempDist < dist)
                    {
                        dist = tempDist;
                        record_t = t;
                        recordBezierCurveDot = bezierCurveDot;
                    }
                }

                if (dist > minBezierDistThreshold)
                {
                    if (lastBezierSegmentInfo.SegmentIndex != i || (lastBezierSegmentInfo.SegmentIndex == i && dist > minRecordBezierDistThreshold))
                    {
                        continue;
                    }
                }

                lastBezierSegmentInfo.SegmentIndex = i;
                lastBezierSegmentInfo.t = record_t;
                lastBezierSegmentInfo.dist = dist;
                lastBezierSegmentInfo.position = recordBezierCurveDot;


                return lastBezierSegmentInfo;
            }

            lastBezierSegmentInfo.SegmentIndex = -1;

            return lastBezierSegmentInfo;
        }


        void AutoSetControlPointIfEnable(int index) {
            creator.SmoothCtrlPoints(index - 3, index + 3);
        }

        void OnEnable()
        {
            creator = (SnakePathCreator)target;
            lastBezierSegmentInfo = new BezierSegmentInfo();
            //LastTool = Tools.current;
            //Tools.current = Tool.None;
        }


        void OnDisable()
        {
            //Tools.current = LastTool;
        }

        private struct BezierSegmentInfo {
            public int SegmentIndex;
            public float t;
            public float dist;
            public Vector3 position;
        }

    }
}