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
        private int SegmentLength = 1;

        [SerializeField, Range(1, 10)]
        private int SegmentPieces = 1;

        [SerializeField, Range(0.1f, 1f)]
        private float Size = 1f;

        private Mesh mesh;

        List<Vector3> vertices;
        List<int> triangles;
        List<Vector2> uv;

        //For Cache purpose
        List<Vector3> procedural_vertices;
        List<int> procedural_triangles;
        List<Vector2> procedural_uv;
        Types.MeshInfo cacheMeshInfo;

        private void Awake()
        {
            mesh = new Mesh();
            _MeshFilter.mesh = mesh;
            cacheMeshInfo = new Types.MeshInfo();

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();

            procedural_vertices = new List<Vector3>();
            procedural_triangles = new List<int>();
            procedural_uv = new List<Vector2>();
        }

        private void Start()
        {
            CreateSegmentLine(SegmentPieces, SegmentLength);
        }

        private void Update()
        {
            CreateSegmentLine(SegmentPieces, SegmentLength);
        }

        private void CreateSegmentLine(int pieces, int segments) {
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();
            uv.Clear();

            for (int p = 0; p < pieces; p++) {
                cacheMeshInfo = CreateIndividualSegment(p, segments);

                vertices.AddRange(cacheMeshInfo.vertices);
                triangles.AddRange(cacheMeshInfo.triangles);
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

        private Types.MeshInfo CreateIndividualSegment(int peiceIndex, int segments) {
            procedural_triangles.Clear();
            procedural_vertices.Clear();

            Vector3 SizeDelimitor = new Vector3(Size, Size, 1f);

            //Segment Length + Space Between each segment
            float startZPos = (peiceIndex * segments * SizeDelimitor.x) + (SizeDelimitor.x * peiceIndex);

            for (int i = 0; i < segments; i++)
            {

                float z = (i * SizeDelimitor.x) + startZPos;

                procedural_vertices.Add(Vector3.Scale(new Vector3(0, 1, z), SizeDelimitor));
                procedural_vertices.Add(Vector3.Scale(new Vector3(1, -1, z), SizeDelimitor));
                procedural_vertices.Add(Vector3.Scale(new Vector3(-1, -1, z), SizeDelimitor));
            }


            //Previous Body Segment Triangles
            int startStep = (peiceIndex * 3 * segments);

            //Head Face
            if (procedural_vertices.Count > 0)
            {
                procedural_triangles.Add(startStep);
                procedural_triangles.Add(startStep+1);
                procedural_triangles.Add(startStep+2);
            }

            for (int i = 1; i < segments; i++)
            {                
                int step = startStep + (i * 3);
                int previousTop = step - 3, previousRight = step - 2, previousLeft = step - 1;
                int currentTop = step, currentRight = step + 1, currentLeft = step + 2;

                //Right Side
                procedural_triangles.Add(previousRight);
                procedural_triangles.Add(previousTop);
                procedural_triangles.Add(currentTop);

                procedural_triangles.Add(currentTop);
                procedural_triangles.Add(currentRight);
                procedural_triangles.Add(previousRight);

                //Bottom Side
                procedural_triangles.Add(previousRight);
                procedural_triangles.Add(previousLeft);
                procedural_triangles.Add(currentRight);

                procedural_triangles.Add(currentRight);
                procedural_triangles.Add(previousLeft);
                procedural_triangles.Add(currentLeft);

                ////Left Side
                procedural_triangles.Add(previousTop);
                procedural_triangles.Add(previousLeft);
                procedural_triangles.Add(currentLeft);

                procedural_triangles.Add(currentTop);
                procedural_triangles.Add(previousTop);
                procedural_triangles.Add(currentLeft);
            }
         
            cacheMeshInfo.vertices = procedural_vertices;
            cacheMeshInfo.triangles = procedural_triangles;

            return cacheMeshInfo;
        }

        //Vector3 is between -1 to 1, UV should be 0 -1 
        private Vector2 NVector3To2(Vector3 vector) {

            vector.Normalize();
            return  (vector + Vector3.one) * 0.5f;

            //return new Vector2((vector.x + 1) * 0.5f,
            //    (vector.y + 1) * 0.5f);
        }
    }
}