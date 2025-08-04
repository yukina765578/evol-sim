Shader "EvolutionSimulator/CircleInstanced"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 1, 1)
        _CircleRadius ("Circle Radius", Range(0.01, 1.0)) = 0.5
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
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _CircleRadius;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                // Transform position using instance matrix
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Calculate distance from center (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float2 uv = input.uv;
                float dist = distance(uv, center);
                
                // Create circle with smooth edge
                float circle = 1.0 - smoothstep(_CircleRadius - 0.01, _CircleRadius, dist);
                
                // Discard pixels outside circle
                if (circle < 0.01)
                    discard;
                
                // Return blue color with alpha
                return float4(_Color.rgb, _Color.a * circle);
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
}
