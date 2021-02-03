using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Hsinpa.Snake
{
    public class SnakeMeshGenerator
    {
        List<Vector3> vertices;
        List<int> triangles;
        List<Vector2> uv;
        Types.MeshInfo meshInfo;

        public float meshSize = 1f;

        public SnakeMeshGenerator() {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();
            meshInfo = new Types.MeshInfo();
        }

        public Types.MeshInfo RenderSegments(Snake.SnakePath snakePath, int startSegmentIndex, int endSegmentIndex) {
            vertices.Clear();
            triangles.Clear();
            uv.Clear();

            int segmentChunk = 10;
            float SizeDelimitor = 1 * meshSize;
            for (int i = 0; i < snakePath.NumSegments; i++)
            {
                int z = i;
                //Ingore index 0 if not in first segment
                int startIndex = (i == 0) ? 0 : 1;

                Types.BezierSegmentInfo[] bezierInfo = snakePath.GetSegmentBezierSteps(i);
                //int bezierInfo

                for (int k = startIndex; k < segmentChunk; k++) {
                    Vector3 faceDir = Vector3.forward;

                    if (k > 0) {
                        faceDir =  (bezierInfo[k].Position - bezierInfo[k - 1].Position).normalized;
                        //Debug.Log(faceDir);
                    }

                    Vector3 topVertice = new Vector3(bezierInfo[k].Position.x, bezierInfo[k].Position.y + SizeDelimitor, bezierInfo[k].Position.z);
                    Vector3 rightVertice = new Vector3(bezierInfo[k].Position.x + SizeDelimitor, bezierInfo[k].Position.y - SizeDelimitor, bezierInfo[k].Position.z);
                    Vector3 leftVertice = new Vector3(bezierInfo[k].Position.x - SizeDelimitor, bezierInfo[k].Position.y - SizeDelimitor, bezierInfo[k].Position.z);

                    Vector3 normal = Vector3.Cross(leftVertice - topVertice, rightVertice - topVertice).normalized;
                    Vector3 normalDiff = faceDir - normal;

                    Quaternion rotation = Quaternion.Euler(faceDir.x, faceDir.y, faceDir.z);
                    Matrix4x4 m = Matrix4x4.Rotate(rotation);
                    topVertice = m.MultiplyPoint3x4(topVertice);
                    rightVertice = m.MultiplyPoint3x4(rightVertice);
                    leftVertice = m.MultiplyPoint3x4(leftVertice);

                    vertices.Add(topVertice);
                    vertices.Add(rightVertice);
                    vertices.Add(leftVertice);
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
                uv.Add(NVector3To2(vertices[i]));
            }

            meshInfo.vertices = vertices;
            meshInfo.triangles = triangles;
            meshInfo.uv = uv;

            return meshInfo;
        }

        //Vector3 is between -1 to 1, UV should be 0 -1 
        private Vector2 NVector3To2(Vector3 vector)
        {
            return (vector + Vector3.one) * 0.5f;
        }
    }
}
