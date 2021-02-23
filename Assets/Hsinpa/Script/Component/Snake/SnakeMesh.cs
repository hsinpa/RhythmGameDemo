using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Snake {

    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class SnakeMesh : MonoBehaviour
    {
        [SerializeField]
        private SnakePath _snakePath;
        public SnakePath snakePath => _snakePath;

        [SerializeField]
        private MeshFilter _meshFilter;

        [SerializeField]
        private MeshRenderer _meshRenderer;

        [SerializeField, Range(0.05f, 1f)]
        public float meshSize = 1f;

        private Mesh _mesh;
        private Material _material;
        private MaterialPropertyBlock m_PropertyBlock;

        private SnakeMeshGenerator _snakeMeshGenerator;
        public SnakeMeshGenerator snakeMeshGenerator => _snakeMeshGenerator;

        public void SetUp() {
            _snakeMeshGenerator = new SnakeMeshGenerator();
            InitMeshIfNeeded();
            m_PropertyBlock = new MaterialPropertyBlock();
            _material = _meshRenderer.material;
        }

        public void SetSnakePath(SnakePath p_snakePath) {
            this._snakePath = p_snakePath;

            m_PropertyBlock.SetColor(EventFlag.SnakeShaderVar.Color, Types.GetColorBySnakeTag(p_snakePath.tag));
            _meshRenderer.SetPropertyBlock(m_PropertyBlock);
        }

        public void RenderMesh()
        {
            if (_snakePath == null) return;

            _snakeMeshGenerator.meshSize = meshSize;

            Types.MeshInfo meshInfo = _snakeMeshGenerator.RenderSegments(_snakePath, 0, _snakePath.NumSegments - 1);
            _mesh.Clear();
            _mesh.SetVertices(meshInfo.vertices);
            _mesh.SetTriangles(meshInfo.triangles, 0);
            _mesh.SetUVs(0, meshInfo.uv);

            _mesh.RecalculateNormals();
        }

        private void InitMeshIfNeeded() {
            if (this._mesh == null) {
                this._mesh = new Mesh();
                this._meshFilter.mesh = this._mesh;
            }
        }
    }
}
