using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa {
    public class SnakePathScorer
    {
        private List<SnakePathViewer.NoteStruct> noteList = new List<SnakePathViewer.NoteStruct>();
        private List<CurrentSnakeVertex> _nearestSnakeVertexList = new List<CurrentSnakeVertex>();
        public List<CurrentSnakeVertex> nearestSnakeVertexList => _nearestSnakeVertexList;

        private float _zGoalPosition;
        private float _distThreshold;

        public SnakePathScorer(float zGoalPosition, float distThreshold) {
            this._zGoalPosition = zGoalPosition;
            this._distThreshold = distThreshold;
        }

        public void OnUpdate(List<SnakePathViewer.NoteStruct> p_noteList)
        {
            this.noteList = p_noteList;
            _nearestSnakeVertexList.Clear();

            foreach (SnakePathViewer.NoteStruct node in noteList) {
                int vertexCount = node.snakeMesh.snakeMeshGenerator.midPoints.Count;

                //TODO : Not going to do it here, but it can be optimize by using telophone number finding algorithm.
                for (int i = 0; i < vertexCount; i++) {

                    float worldVertexPos = node.snakeMesh.transform.position.z + node.snakeMesh.snakeMeshGenerator.midPoints[i].z;

                    float distDiff = Mathf.Abs(_zGoalPosition - worldVertexPos);

                    if (worldVertexPos > _zGoalPosition && distDiff < _distThreshold) {

                        CurrentSnakeVertex snakeVertex = new CurrentSnakeVertex();
                        snakeVertex.noteStruct = node;
                        snakeVertex.index = i;

                        _nearestSnakeVertexList.Add(snakeVertex);

                        break;
                    }
                }
            }
        }

        public struct CurrentSnakeVertex {
            public SnakePathViewer.NoteStruct noteStruct;
            public int index;
        }

    }
}