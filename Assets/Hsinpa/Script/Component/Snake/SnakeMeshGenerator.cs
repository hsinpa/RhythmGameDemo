﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Hsinpa.Utility;

namespace Hsinpa.Snake
{
    public class SnakeMeshGenerator
    {
        List<Vector3> vertices;
        List<int> triangles;
        List<Vector2> uv;
        Types.MeshInfo meshInfo;

        public float meshSize = 1f;

        private Dictionary<int, Vector2> UVParternLookupTable = new Dictionary<int, Vector2>();

        private List<Vector3> _midPoints;
        public List<Vector3> midPoints => _midPoints;

        public SnakeMeshGenerator() {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();
            meshInfo = new Types.MeshInfo();
            _midPoints = new List<Vector3>();

            UVParternLookupTable.Add(0, new Vector2(0, 0));
            UVParternLookupTable.Add(1, new Vector2(0, 1));
            UVParternLookupTable.Add(2, new Vector2(1, 0));
            UVParternLookupTable.Add(3, new Vector2(1, 1));
        }

        public Types.MeshInfo RenderSegments(Snake.SnakePath snakePath, int startSegmentIndex, int endSegmentIndex) {
            vertices.Clear();
            triangles.Clear();
            uv.Clear();
            _midPoints.Clear();

            float SizeDelimitor = 1 * meshSize;
            for (int i = 0; i < snakePath.NumSegments; i++)
            {
                //Ingore index 0 if not in first segment
                int startIndex = (i == 0) ? 0 : 1;

                List<Types.BezierSegmentInfo> bezierInfo = snakePath.GetSegmentBezierSteps(i);
                int segmentChunk = bezierInfo.Count;
                //int bezierInfo

                for (int k = startIndex; k < segmentChunk; k++) {
                    Vector3 faceDir = Vector3.forward;
                    Vector3 rightDir = Vector3.right;

                    if (k > 0)
                    {
                        faceDir = (bezierInfo[k].Position - bezierInfo[k - 1].Position).normalized;

                        rightDir = new Vector3(-(bezierInfo[k - 1].Position.z - bezierInfo[k].Position.z),
                            0, bezierInfo[k - 1].Position.x - bezierInfo[k].Position.x).normalized;
                    }
                    else {
                        faceDir = (bezierInfo[k+1].Position - bezierInfo[k].Position).normalized;

                        rightDir = new Vector3(-(bezierInfo[k].Position.z - bezierInfo[k+1].Position.z),
                            0, bezierInfo[k].Position.x - bezierInfo[k+1].Position.x).normalized;
                    }

                    Vector3 topDir = Vector3.Cross(faceDir, rightDir).normalized;

                    Vector3 topVertice = bezierInfo[k].Position + (topDir * SizeDelimitor);
                    Vector3 rightVertice = bezierInfo[k].Position + (rightDir * SizeDelimitor - (topDir * SizeDelimitor));
                    Vector3 leftVertice = bezierInfo[k].Position + (-rightDir * SizeDelimitor - (topDir * SizeDelimitor));

                    //Vector3 normal = Vector3.Cross(leftVertice - topVertice, rightVertice - topVertice).normalized;

                    vertices.Add(topVertice);
                    vertices.Add(rightVertice);
                    vertices.Add(leftVertice);
                    _midPoints.Add(bezierInfo[k].Position);
                }
            }

            int verticesCount = vertices.Count;

            //Head Face
            if (vertices.Count > 0)
            {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            }

            int segmentCount = Mathf.FloorToInt(verticesCount / 3f);

            for (int i = 1; i < segmentCount; i++)
            {
                int step = i * 3;
                int previousTop = step - 3, previousRight = step - 2, previousLeft = step - 1;
                int currentTop = step, currentRight = step + 1, currentLeft = step + 2;

                //Right Side
                triangles.Add(previousRight);
                triangles.Add(previousTop);
                triangles.Add(currentTop);

                triangles.Add(currentTop);
                triangles.Add(currentRight);
                triangles.Add(previousRight);

                //Bottom Side
                triangles.Add(previousRight);
                triangles.Add(previousLeft);
                triangles.Add(currentRight);

                triangles.Add(currentRight);
                triangles.Add(previousLeft);
                triangles.Add(currentLeft);

                ////Left Side
                triangles.Add(previousTop);
                triangles.Add(previousLeft);
                triangles.Add(currentLeft);

                triangles.Add(currentTop);
                triangles.Add(previousTop);
                triangles.Add(currentLeft);
            }

            //int vLength = vertices.Count;
            for (int i = 0; i < verticesCount; i++)
            {
                uv.Add(NVector3To2(vertices[i], i) * 0.5f);
            }

            meshInfo.vertices = vertices;
            meshInfo.triangles = triangles;
            meshInfo.uv = uv;

            return meshInfo;
        }

        //Vector3 is between -1 to 1, UV should be 0 -1 
        private Vector2 NVector3To2(Vector3 vector, int index)
        {
            if (index < 3) {
                return new Vector2(vector.x, vector.y);
            }

            return new Vector2(vector.y, vector.z);
        }
    }
}
