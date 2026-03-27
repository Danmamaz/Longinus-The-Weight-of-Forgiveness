Shader "Custom/OptimizedEarthURP"
{
    Properties
    {
        _TopColor ("Top Earth Color (Light)", Color) = (0.45, 0.35, 0.2, 1)
        _BottomColor ("Bottom Earth Color (Dark)", Color) = (0.2, 0.15, 0.1, 1)
        _HeightThreshold ("Height Threshold (World Y)", Float) = 0.0
        _FadeRange ("Fade Range", Float) = 0.5
        _PixelSize ("World Pixel Size (0 for smooth)", Float) = 0.0625
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        // =============================================
        // PASS 1 — Основний колір
        // =============================================
        Pass
        {
            Name "EarthPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float _HeightThreshold;
                float _FadeRange;
                float _PixelSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float yPos = IN.positionWS.y;
                if (_PixelSize > 0.0)
                {
                    yPos = floor(yPos / _PixelSize) * _PixelSize;
                }

                float weight = saturate((_HeightThreshold - yPos) / _FadeRange);
                half3 finalColor = lerp(_TopColor.rgb, _BottomColor.rgb, weight);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // =============================================
        // PASS 2 — Shadow Caster (Ручна реалізація)
        // Виправляємо помилку "incompatible keyword space"
        // =============================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            ShadowVaryings ShadowVert(ShadowAttributes IN)
            {
                ShadowVaryings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // Використовуємо стандартне зміщення тіней URP
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = positionCS;
                return OUT;
            }

            half4 ShadowFrag(ShadowVaryings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}