Shader "Hidden/PixelateScreen"
{
    Properties
    {
        _PixelDensity("Pixel Density", Float) = 64.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        // VITAL: Force this to draw on top of everything
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Pixelate"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                // We only need the ID to generate a full screen triangle/quad
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _PixelDensity;
            
            // Texture definitions
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // This Vertex Shader manually constructs a full-screen triangle
            // It works even if the camera matrices are messed up.
            Varyings Vert(Attributes input)
            {
                Varyings output;
                
                // Hardcoded Full Screen Triangle UVs
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
                
                output.positionCS = pos;
                output.uv = uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // 1. ASPECT RATIO FIX
                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;

                // 2. PIXELATION MATH
                uv = floor(uv * _PixelDensity) / _PixelDensity;

                // 3. RESTORE ASPECT
                uv.x /= aspect;

                // 4. FORCE RED TEST (Uncomment to test)
                // return half4(1, 0, 0, 1);

                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
            }
            ENDHLSL
        }
    }
}