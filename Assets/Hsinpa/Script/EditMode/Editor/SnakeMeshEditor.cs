using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hsinpa.Snake;

namespace Hsinpa.Creator
{
    [CustomEditor(typeof(SnakeMesh))]
    public class SnakeMeshEdtior : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

    }
}
