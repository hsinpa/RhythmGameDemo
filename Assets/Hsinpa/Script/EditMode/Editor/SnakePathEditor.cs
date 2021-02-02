using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hsinpa.Snake;

namespace Hsinpa.Creator
{
    [CustomEditor(typeof(SnakePathCreator))]
    public class SnakePathEditor : Editor
    {
        SnakePathCreator creator;
        Tool LastTool = Tool.None;

        Vector3 _snap = Vector3.one * 0.5f;

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


        }

        void Draw(Event guiEvent) {

            if (creator.snakePath != null) {

                for (int i = 0; i < creator.snakePath.NumSegments; i++)
                {
                    Vector3[] points = creator.snakePath.GetPointsInSegment(i);
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);
                    Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
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

        void AutoSetControlPointIfEnable(int index) {
            creator.SmoothCtrlPoints(index - 3, index + 3);
        }

        void OnEnable()
        {
            creator = (SnakePathCreator)target;
            //LastTool = Tools.current;
            //Tools.current = Tool.None;
        }


        void OnDisable()
        {
            //Tools.current = LastTool;
        }


    }
}