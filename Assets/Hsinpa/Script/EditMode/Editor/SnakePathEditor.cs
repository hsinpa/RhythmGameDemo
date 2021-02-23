using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hsinpa.Snake;
using HUtil = Hsinpa.Utility;
using GUtil = Utility;
using System;

namespace Hsinpa.Creator
{
    [CustomEditor(typeof(SnakePathCreator))]
    public class SnakePathEditor : Editor
    {
        SnakePathCreator creator;
        Vector3 parentPos => creator.transform.position;

        Vector3 _snap = Vector3.one * 0.5f;
        float minBezierDistThreshold = 0.2f;

        Types.BezierSegmentInfo lastBezierSegmentInfo;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SnakePathCreator myScript = (SnakePathCreator)target;
            if (GUILayout.Button("Default Snake Layout"))
            {
                Undo.RecordObject(creator.snakePath, "Create Default Layout");

                myScript.CreateBasicPathSetup();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Render Mesh"))
            {
                myScript.RenderPathLayoutToMesh();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Revert", GUILayout.ExpandWidth(true)))
                RevertSnakePath(new Vector3(-1, -1, 1), creator.snakePath);

            if (GUILayout.Button("Revert Y", GUILayout.ExpandWidth(true)))
                RevertSnakePath(new Vector3(1, -1, 1), creator.snakePath);

            if (GUILayout.Button("Revert X", GUILayout.ExpandWidth(true)))
                RevertSnakePath(new Vector3(-1, 1, 1), creator.snakePath);

            GUILayout.EndHorizontal();

            bool isEnableAutoCtrlP = GUILayout.Toggle(creator.enableAutoContorlPoint, "Enable Auto Ctrl Point");
            if (isEnableAutoCtrlP != creator.enableAutoContorlPoint) {
                Undo.RecordObject(creator.snakePath, "Enable Auto Ctrl Point");
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

            //Only allow adding segment on orthographic mode and 100% along one camera axis
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && IsAddSegmentAvailable(direction))
            {
                
                Vector3 lastPoint = ConvertToWorldPos(creator.snakePath[creator.snakePath.PointCount - 1]);
                //float magnitude = Vector3.Distance(lastPoint, mousePos);

                //Vector3 finalPoint = mousePos + (direction * magnitude);
                Vector3 finalPoint = RestrictPositionBaseOnCameraDir(direction, mousePos, lastPoint);

                Undo.RecordObject(creator.snakePath, "Add segment");
                creator.snakePath.AddSegment(ConvertToLocalPos(finalPoint));

                AutoSetControlPointIfEnable(creator.snakePath.PointCount - 1);
            }

            //Delete Anchor
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1) {

                int removeIndex = FindTheMostClosetIndex(mousePos, direction);

                if (removeIndex >= 0) {

                    Undo.RecordObject(creator.snakePath, "Delete segment");
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
                    Undo.RecordObject(creator.snakePath, "Insert segment");

                    creator.snakePath.SplitSegment(ConvertToLocalPos(lastBezierSegmentInfo.Position), lastBezierSegmentInfo.SegmentIndex);

                    int anchorIndex = lastBezierSegmentInfo.SegmentIndex * 3;
                    creator.SmoothCtrlPoints(anchorIndex - 3, anchorIndex + 3);
                    SceneView.RepaintAll();
                }
            }

            if (!guiEvent.control)
                lastBezierSegmentInfo.SegmentIndex = -1;
        }

        void Draw(Event guiEvent) {

            if (creator.snakePath != null) {

                creator.snakePath.OffsetPosition = creator.transform.position;

                for (int i = 0; i < creator.snakePath.NumSegments; i++)
                {
                    Vector3[] points = creator.snakePath.GetPointsInSegment(i);
                    points = ConvertArrayToWorldPos(points);

                    Handles.color = Color.black;

                    Vector3 worldPos = ConvertToWorldPos(creator.snakePath[i]);
                    Vector3 newPos = Handles.FreeMoveHandle(worldPos, Quaternion.identity, .2f, _snap, Handles.CylinderHandleCap);

                    Handles.DrawLine(points[1], (points[0]));
                    Handles.DrawLine(points[2], (points[3]));

                    Color bezierColor = (i == lastBezierSegmentInfo.SegmentIndex) ? Color.yellow : Color.green;

                    if (i == lastBezierSegmentInfo.SegmentIndex) {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, lastBezierSegmentInfo.Position, Quaternion.identity, 0.2f, EventType.Repaint);
                    }

                    Handles.DrawBezier(points[0], points[3], points[1], points[2], bezierColor, null, 2);
                }


                for (int i = 0; i < creator.snakePath.PointCount; i++) {
                    bool isAnchorP = SnakePath.IsAnchorPoint(i);
                    Handles.color = (isAnchorP) ? Color.red : Color.blue;

                    Vector3 worldPos = ConvertToWorldPos(creator.snakePath[i]);
                    Vector3 newPos = Handles.FreeMoveHandle(worldPos, Quaternion.identity, .2f, _snap, Handles.CylinderHandleCap);

                    Vector3 LocalPos = ConvertToLocalPos(newPos);

                    if (isAnchorP)
                        LocalPos = ClampPositionToConstraint(LocalPos);


                    //Move Anchor Around
                    if (creator.snakePath[i] != LocalPos && (!creator.enableAutoContorlPoint || ((creator.enableAutoContorlPoint && isAnchorP) ||
                        //The first / last control point leave to player to decide
                        i <= 1 || i >= creator.snakePath.PointCount - 2)))
                    {
                        Undo.RecordObject(creator.snakePath, "Move point");

                        creator.snakePath.UpdateAnchor(LocalPos, i);

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
                Vector3 targetToCamDir = ( ConvertToWorldPos(creator.snakePath[i]) - mousePos).normalized;
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

        Types.BezierSegmentInfo FindClosestSegment(Vector3 mousePos, Vector3 mouseDir) {
            Vector3 recordBezierCurveDot = Vector3.zero;

            for (int i = 0; i < creator.snakePath.NumSegments; i++)
            {
                Vector3[] points = creator.snakePath.GetPointsInSegment(i);
                points = ConvertArrayToWorldPos(points);

                float dist = 1000;
                float record_t = 0;

                for (float t = 0.1f; t < 1; t += 0.1f)
                {
                    Vector3 bezierCurveDot = HUtil.SnakeUtility.BezierCurve(points[0], points[1], points[2], points[3], t);
                    float distToCamera = (bezierCurveDot - mousePos).magnitude;
                    Vector3 simulateMousePoint = mousePos + (distToCamera * mouseDir);

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
                    continue;
                }

                lastBezierSegmentInfo.SegmentIndex = i;
                lastBezierSegmentInfo.Position = recordBezierCurveDot;

                return lastBezierSegmentInfo;
            }

            lastBezierSegmentInfo.SegmentIndex = -1;

            return lastBezierSegmentInfo;
        }


        void AutoSetControlPointIfEnable(int index) {
            creator.SmoothCtrlPoints(index - 3, index + 3);
        }

        bool HasPositionInsideConstraint(Vector3 position) {
            return (position.x >= creator.XAxisConstraints.x && position.x <= creator.XAxisConstraints.y) &&
                    (position.y >= creator.YAxisConstraints.x && position.y <= creator.YAxisConstraints.y);
        }

        Vector3 ConvertToWorldPos(Vector3 p_vector) {
            return parentPos + p_vector;
        }

        Vector3[] ConvertArrayToWorldPos(Vector3[] p_vectors)
        {
            int count = p_vectors.Length;

            for (int i = 0; i < count; i++) {
                p_vectors[i] = p_vectors[i] + parentPos;
            }

            return p_vectors;
        }

        Vector3 ConvertToLocalPos(Vector3 p_vector) {
            return p_vector - parentPos;
        }

        Vector3 ClampPositionToConstraint(Vector3 position)
        {
            return new Vector3(Mathf.Clamp(position.x, creator.XAxisConstraints.x, creator.XAxisConstraints.y),
                                Mathf.Clamp(position.y, creator.YAxisConstraints.x, creator.YAxisConstraints.y), position.z);
        }

        bool IsAddSegmentAvailable(Vector3 cameraDir)
        {
            float mod1 = (cameraDir.x % 1f) + (cameraDir.y % 1f) + (cameraDir.z % 1f);
            float round2Decimal = (float)Math.Round(mod1 * 100f) / 100f;

            return round2Decimal == 0;
        }

        Vector3 RestrictPositionBaseOnCameraDir(Vector3 cameraDir, Vector3 mousePos, Vector3 lastAnchorPoint) {
            Vector3 absCameraDir = new Vector3(Mathf.Abs(cameraDir.x), Mathf.Abs(cameraDir.y), Mathf.Abs(cameraDir.z));
            lastAnchorPoint.Scale(absCameraDir);

            Vector3 offset = new Vector3(1 - absCameraDir.x, 1 - absCameraDir.y, 1 - absCameraDir.z);

            mousePos.Scale(offset);

            return mousePos + (lastAnchorPoint);
        }

        /// <summary>
        /// Copy the snake path data and revert it
        /// </summary>
        private void RevertSnakePath(Vector3 revertDirection, SnakePath copyPath) {
            SnakePath emptySnakepath = ScriptableObject.CreateInstance<SnakePath>();
            string fileName = copyPath.name + "_Clone.asset";
            AssetDatabase.CreateAsset(emptySnakepath, EventFlag.Path.SnakePathSRPFolder + fileName);

            int pointCount = copyPath.PointCount;
            for (int i = 0; i < pointCount; i++)
            {

                float yOffset = creator.YAxisConstraints.y - copyPath[i].y;
                float revertYPos = creator.YAxisConstraints.x + yOffset;

                float xOffset = creator.XAxisConstraints.y - copyPath[i].x;
                float revertXPos = creator.XAxisConstraints.x + xOffset;

                var revertPath = new Vector3( (revertDirection.x == -1) ? revertXPos : copyPath[i].x,
                                (revertDirection.y == -1) ? revertYPos : copyPath[i].y,
                                copyPath[i].z
                    );

                emptySnakepath.Push(revertPath);
            }

            var snakeCreator = GUtil.UtilityMethod.CreateObjectToParent<SnakePathCreator>(creator.transform.parent, creator.transform.gameObject);
            snakeCreator.transform.position = creator.transform.position;
            snakeCreator.SetSnakePath(emptySnakepath);

            AssetDatabase.SaveAssets();
        }

        void OnEnable()
        {
            creator = (SnakePathCreator)target;
            lastBezierSegmentInfo = new Types.BezierSegmentInfo();

            if (creator.snakePath != null)
                creator.snakePath.OffsetPosition = creator.transform.position;
        }
    }
}