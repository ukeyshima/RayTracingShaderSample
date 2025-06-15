using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracingShaderSample
{
    public class RayTraceManager : MonoBehaviour
    {
        [SerializeField] private RayTracingShader _rayTracingShader;
        [SerializeField] private Renderer[] _sceneRenderers; 
        [SerializeField] private GameObject[] _instanceingRenderers;
        [SerializeField] private Camera _mainCamera;
        
        private RayTracingAccelerationStructure _accelerationStructure;
        private RenderTexture _target;

        private void Start()
        {
            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();

            RayTracingAccelerationStructure.Settings settings = new RayTracingAccelerationStructure.Settings()
            {
                enableCompaction = true,
                managementMode = RayTracingAccelerationStructure.ManagementMode.Manual,
                rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything
            };
            _accelerationStructure = new RayTracingAccelerationStructure(settings);

            RayTracingSubMeshFlags[] subMeshFlags = { RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.ClosestHitOnly };

            uint instanceID = 0;
            foreach (var renderer in _sceneRenderers)
            {
                _accelerationStructure.AddInstance(
                    renderer,
                    subMeshFlags,
                    true,
                    false,
                    0xff,
                    0
                );
            }
            
            foreach (var renderer in _instanceingRenderers)
            {
                var instancing = renderer.GetComponent<IRayTraceInstancingRenderer>();
                instancing.Init();
                RayTracingMeshInstanceConfig config = instancing.RayTracingMeshInstanceConfig;
                _accelerationStructure.AddInstancesIndirect(
                    config, 
                    instancing.TransformBuffer, 
                    instancing.TransformBuffer.count, 
                    instancing.ArgsBuffer, 
                    0, 
                    0
                    );
            }
            
            BuildAccelerationStructure();
            Debug.Log("Acceleration structure created with " + _accelerationStructure.GetInstanceCount() + " instances.");
        }

        private void OnDestroy()
        {
            if (_target != null)
            {
                _target.Release();
            }
            if (_accelerationStructure != null)
            {
                _accelerationStructure.Release();
            }
        }
        
        private void BuildAccelerationStructure()
        {
            _accelerationStructure.Build();
        }

        private void UpdateAccelerationStructure()
        {
            foreach (var renderer in _sceneRenderers)
            {
                _accelerationStructure.UpdateInstanceTransform(renderer);
            }

            BuildAccelerationStructure();
        }

        private void RenderRayTrace()
        {
            UpdateAccelerationStructure();

            _rayTracingShader.SetShaderPass("Closesthit");
            _rayTracingShader.SetTexture("_Output", _target);
            _rayTracingShader.SetAccelerationStructure("_SceneAccelStruct", _accelerationStructure);
            _rayTracingShader.SetFloat("_Zoom", Mathf.Tan(Mathf.Deg2Rad * _mainCamera.fieldOfView * 0.5f));
            _rayTracingShader.Dispatch("MainRayGenShader", _target.width, _target.height, 1, _mainCamera);
        }
        
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderRayTrace();
            Graphics.Blit(_target, destination);
        }
    }
}