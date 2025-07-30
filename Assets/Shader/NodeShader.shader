Shader "EvolutionSimulator/NodeInstanced"
{
    Properties
    {
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct NodeRenderData
            {
                float3 position;
                float size;
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
            StructuredBuffer<NodeRenderData> _NodeData;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                NodeRenderData nodeData = _NodeData[input.instanceID];
                
                // Scale quad by node size and position at node location
                float3 worldPos = input.positionOS.xyz * nodeData.size + nodeData.position;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.color = nodeData.color;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centered = input.uv - 0.5;
                
                // Calculate distance from center
                float distance = length(centered);
                
                // Create circle with anti-aliased edge
                float circle = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, distance);
                
                // Apply color and alpha
                float4 color = input.color;
                color.a *= circle;
                
                // Discard transparent pixels
                if (color.a < 0.01)
                    discard;
                
                return color;
            }
            ENDHLSL
        }
    }
}
