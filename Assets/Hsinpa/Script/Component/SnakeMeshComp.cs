using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa {
    [ExecuteInEditMode]
    public class SnakeMeshComp : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter _MeshFilter;

        [SerializeField]
        private MeshRenderer _MeshRenderer;

        [SerializeField, Range(1, 10)]
        private int SegmentLength;

        [SerializeField, Range(0.1f, 1f)]
        private float Size = 1f;

        private Mesh mesh;

        List<Vector3> vertices;
        List<int> triangles;
        List<Vector2> uv;

        private void Awake()
        {
            mesh = new Mesh();
            _MeshFilter.mesh = mesh;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();
        }

        private void Start()
        {
            CreateSegmentLine(SegmentLength);
        }

        private void Update()
        {
            CreateSegmentLine(SegmentLength);
        }

        private void CreateSegmentLine(int segments) {
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();
            uv.Clear();

            Vector3 SizeDelimitor = new Vector3(Size, Size, 1f);
            for (int i = 0; i < segments; i++) {

                int z = i;

                vertices.Add(Vector3.Scale(new Vector3(0, 1, z), SizeDelimitor));
                vertices.Add(Vector3.Scale(new Vector3(1, -1, z), SizeDelimitor));
                vertices.Add(Vector3.Scale(new Vector3(-1, -1, z), SizeDelimitor));
            }

            //Head Face
            if (vertices.Count > 0) {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            }


            for (int i = 1; i < segments; i++)
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

            int vLength = vertices.Count;
            for (int i = 0; i < vLength; i++)
            {
                uv.Add(NVector3To2(vertices[i]));
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uv);

            mesh.RecalculateNormals();

        }

        //Vector3 is between -1 to 1, UV should be 0 -1 
        private Vector2 NVector3To2(Vector3 vector) {

            return  (vector + Vector3.one) * 0.5f;

            //return new Vector2((vector.x + 1) * 0.5f,
            //    (vector.y + 1) * 0.5f);
        }
    }
}