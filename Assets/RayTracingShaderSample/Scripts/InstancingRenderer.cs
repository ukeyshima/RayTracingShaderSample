using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracingShaderSample
{
    public class InstancingRenderer : MonoBehaviour, IRayTraceInstancingRenderer
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private int _instanceCount = 1000;

        private GraphicsBuffer _instanceBuffer;
        private GraphicsBuffer _argsBuffer;

        public GraphicsBuffer TransformBuffer => _instanceBuffer;
        public GraphicsBuffer ArgsBuffer => _argsBuffer;

        public RayTracingMeshInstanceConfig RayTracingMeshInstanceConfig => new RayTracingMeshInstanceConfig
        {
            mesh = _mesh,
            material = _material
        };

        public void Init()
        {
            _instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _instanceCount, Marshal.SizeOf(typeof(Matrix4x4)));
            _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, Marshal.SizeOf(typeof(uint)) * 5);
            Matrix4x4[] matrices = new Matrix4x4[_instanceCount];
            for (int i = 0; i < _instanceCount; i++)
            {
                matrices[i] = Matrix4x4.TRS(
                    new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f)),
                    Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)),
                    Vector3.one * Random.Range(0.5f, 2f)
                );
            }

            _instanceBuffer.SetData(matrices);
            uint[] args = new uint[5];
            args[0] = _mesh.GetIndexCount(0);
            args[1] = (uint)_instanceCount;
            args[2] = _mesh.GetIndexStart(0);
            args[3] = _mesh.GetBaseVertex(0);
            args[4] = 0;
            _argsBuffer.SetData(args);
        }

        private void OnDestroy()
        {
            if (_instanceBuffer != null)
            {
                _instanceBuffer.Release();
            }
            if (_argsBuffer != null)
            {
                _argsBuffer.Release();
            }
        }
    }
}