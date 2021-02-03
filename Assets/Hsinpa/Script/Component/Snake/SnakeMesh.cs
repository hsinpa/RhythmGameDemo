using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hsinpa.SnakeMesh;

namespace Hsinpa.Snake {

    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class SnakeMesh : MonoBehaviour
    {
        [SerializeField]
        private SnakePath _snakePath;

        [SerializeField]
        private MeshFilter _meshFilter;

        [SerializeField]
        private MeshRenderer _meshRenderer;

        private Mesh _mesh;

        private SnakeMeshGenerator _snakeMeshGenerator;

        public void SetUp() {
            _snakeMeshGenerator = new SnakeMeshGenerator();
            InitMeshIfNeeded();
        }

        public void SetSnakePath(SnakePath p_snakePath) {
            this._snakePath = p_snakePath;
        }

        public void RenderMesh()
        {
            if (_snakePath == null) return;

            InitMeshIfNeeded();


            for (int i = 0; i < _snakePath.NumSegments; i++) {

            }
        }

        private void InitMeshIfNeeded() {
            if (this._mesh == null) {
                this._mesh = new Mesh();
                this._meshFilter.mesh = this._mesh;
            }
        }
    }
}
