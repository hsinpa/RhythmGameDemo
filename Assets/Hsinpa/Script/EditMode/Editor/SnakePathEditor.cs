using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
            Input();
            Draw();
        }

        void Input()
        {
            Event guiEvent = Event.current;
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
            Vector3 direction = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).direction;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                Vector3 lastPoint = creator.snakePath[creator.snakePath.PointCount - 1];
                float magnitude = Vector3.Distance(lastPoint, mousePos);

                Vector3 finalPoint = mousePos + (direction * magnitude);

                Undo.RecordObject(creator, "Add segment");
                creator.snakePath.AddSegment(finalPoint);
            }
        }

        void Draw() {

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
                    Handles.color = Color.red;

                    Vector3 newPos = Handles.FreeMoveHandle(creator.snakePath[i], Quaternion.identity, .2f, _snap, Handles.CylinderHandleCap);

                    if (creator.snakePath[i] != newPos)
                    {
                        Undo.RecordObject(creator, "Move point");

                        creator.snakePath.Update(newPos, i);
                    }
                }
            }
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