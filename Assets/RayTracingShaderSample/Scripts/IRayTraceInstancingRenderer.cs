using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracingShaderSample
{
    public interface IRayTraceInstancingRenderer
    {
        public GraphicsBuffer TransformBuffer { get; }
        public GraphicsBuffer ArgsBuffer { get; }
        public RayTracingMeshInstanceConfig RayTracingMeshInstanceConfig { get; }
        public void Init();
    }
}