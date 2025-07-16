Shader "EvolutionSimulator/FoodCircle"
{
    Properties
    {
        _FoodSize ("Food Size", Float) = 0.5
        _FoodColor ("Food Color", Color) = (0, 0.8, 0, 1)
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
            };

            // Properties
            float _FoodSize;
            float4 _FoodColor;
            float _EdgeSoftness;

            // Buffer from C# script
            StructuredBuffer<float3> _Positions;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Get instance position from buffer
                float3 instancePos = _Positions[input.instanceID];
                
                // Scale the quad by food size and offset by instance position
                float3 worldPos = input.positionOS.xyz * _FoodSize + instancePos;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                
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
                
                // Apply alpha for transparency
                float4 color = _FoodColor;
                color.a *= circle;
                
                // Discard completely transparent pixels for performance
                if (color.a < 0.01)
                    discard;
                
                return color;
            }
            ENDHLSL
        }
    }
}