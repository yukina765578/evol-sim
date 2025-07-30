Shader "EvolutionSimulator/SegmentInstanced"
{
    Properties
    {
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct SegmentRenderData
            {
                float3 startPos;
                float3 endPos;
                float width;
                float4 color;
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD1;
            };

            float _EdgeSoftness;
            StructuredBuffer<SegmentRenderData> _SegmentData;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                SegmentRenderData segmentData = _SegmentData[input.instanceID];
                
                // Calculate segment direction and length
                float3 direction = segmentData.endPos - segmentData.startPos;
                float segmentLength = length(direction);
                float3 normalizedDir = normalize(direction);
                
                // Calculate perpendicular for width
                float3 perpendicular = float3(-normalizedDir.y, normalizedDir.x, 0);
                
                // Transform quad to segment orientation and scale
                float3 localPos = input.positionOS.xyz;
                
                // Scale: x = segmentLength, y = width
                localPos.x *= segmentLength;
                localPos.y *= segmentData.width;
                
                // Orient and position the segment
                float3 worldPos = segmentData.startPos + 
                                  (localPos.x + segmentLength * 0.5) * normalizedDir + 
                                  localPos.y * perpendicular;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.color = segmentData.color;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Simple rectangle with slight edge softening
                float2 centered = abs(input.uv - 0.5);
                float edge = max(centered.x, centered.y);
                float alpha = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, edge);
                
                float4 color = input.color;
                color.a *= alpha;
                
                return color;
            }
            ENDHLSL
        }
    }
}
