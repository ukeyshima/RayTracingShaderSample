Shader "Unlit/Closesthit"
{
    Properties { }
    SubShader
    {
        Pass
        {
            Name "Closesthit"
            Tags{ "LightMode" = "RayTracing" }
        
            HLSLPROGRAM

            #include "UnityRaytracingMeshUtils.cginc"

            #pragma raytracing test
            
            struct AttributeData
            {
                float2 barycentrics;
            };

            struct RayPayload
            {
                float4 color;
                float3 position;
            };

            struct Vertex
            {
                float3 position;
                float3 normal;
                float4 tangent;
                float2 texCoord0;
                float2 texCoord1;
                float2 texCoord2;
                float2 texCoord3;
                float4 color;
            };

            Vertex FetchVertex(uint vertexIndex)
            {
                Vertex v;
                v.position = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributePosition);
                v.normal = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
                v.tangent = UnityRayTracingFetchVertexAttribute4(vertexIndex, kVertexAttributeTangent);
                v.texCoord0 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord0);
                v.texCoord1 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord1);
                v.texCoord2 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord2);
                v.texCoord3 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord3);
                v.color = UnityRayTracingFetchVertexAttribute4(vertexIndex, kVertexAttributeColor);
                return v;
            }

            Vertex InterpolateVertices(Vertex v0, Vertex v1, Vertex v2, float3 barycentrics)
            {
                Vertex v;
                #define INTERPOLATE_ATTRIBUTE(attr) v.attr = v0.attr * barycentrics.x + v1.attr * barycentrics.y + v2.attr * barycentrics.z
                INTERPOLATE_ATTRIBUTE(position);
                INTERPOLATE_ATTRIBUTE(normal);
                INTERPOLATE_ATTRIBUTE(tangent);
                INTERPOLATE_ATTRIBUTE(texCoord0);
                INTERPOLATE_ATTRIBUTE(texCoord1);
                INTERPOLATE_ATTRIBUTE(texCoord2);
                INTERPOLATE_ATTRIBUTE(texCoord3);
                INTERPOLATE_ATTRIBUTE(color);
                return v;
            }
            
            [shader("closesthit")]
            void ClosestHitMain(inout RayPayload payload : SV_RayPayload, AttributeData attribs : SV_IntersectionAttributes)
            {
                uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

                Vertex v0, v1, v2;
                v0 = FetchVertex(triangleIndices.x);
                v1 = FetchVertex(triangleIndices.y);
                v2 = FetchVertex(triangleIndices.z);

                float3 barycentricCoords = float3(1.0 - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
                Vertex v = InterpolateVertices(v0, v1, v2, barycentricCoords);

                float4 color = float4(v.normal * 0.5 + 0.5, 1.0);
                payload.color = color;
                payload.position = mul(ObjectToWorld(), float4(v.position, 1)).xyz;
            }

            ENDHLSL
        }
    }
}