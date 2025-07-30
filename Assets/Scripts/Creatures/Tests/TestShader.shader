Shader "Test/CircleInstancedProcedural"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
        _CircleSize ("Circle Size", Float) = 1.0
        _EdgeSoft ("Edge Softness", Range(0.01, 0.1)) = 0.02
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
            float4 _Color;
            float _CircleSize;
            float _EdgeSoft;
            
            // Buffer from C# script
            StructuredBuffer<float3> _Positions;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Get instance position from buffer
                float3 instancePos = _Positions[input.instanceID];
                
                // Scale quad by circle size and position it
                float3 worldPos = input.positionOS.xyz * _CircleSize + instancePos;
                
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
                float circle = 1.0 - smoothstep(0.5 - _EdgeSoft, 0.5, distance);
                
                // Apply color and alpha
                half4 color = _Color;
                color.a *= circle;
                
                // Discard transparent pixels
                if (color.a < 0.01) discard;
                
                return color;
            }
            ENDHLSL
        }
    }
}
