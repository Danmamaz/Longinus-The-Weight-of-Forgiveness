Shader "Custom/OptimizedEarthURP"
{
    Properties
    {
        [Header(Earth Settings)]
        _TopColor ("Top Earth Color (Light)", Color) = (0.45, 0.35, 0.2, 1)
        _BottomColor ("Bottom Earth Color (Dark)", Color) = (0.2, 0.15, 0.1, 1)
        _DirtColor ("Dirt Variation Color", Color) = (0.3, 0.2, 0.15, 1)
        _HeightThreshold ("Earth Height Threshold", Float) = 0.0
        _FadeRange ("Earth Fade Range", Float) = 0.5
        
        [Header(Grass Settings)]
        _GrassColor ("Grass Base Color", Color) = (0.25, 0.4, 0.15, 1)
        _GrassVariation ("Grass Variation Color", Color) = (0.3, 0.45, 0.2, 1)
        _GrassHeight ("Grass Start Height", Float) = 1.0
        _GrassFade ("Grass Height Fade", Float) = 0.5
        _GrassSlope ("Grass Slope Threshold (Flatness)", Range(0.1, 1.0)) = 0.8

        [Header(Global Style)]
        _PixelSize ("World Pixel Size (0 for smooth)", Float) = 0.0625
        _NoiseScale ("Noise Scale (Variation)", Float) = 0.5
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.3
        _ShadowIntensity ("Shadow Darkness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        // =============================================
        // PASS 1 — Основний колір + Отримання тіней
        // =============================================
        Pass
        {
            Name "EarthPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Ключові слова для підтримки тіней URP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float4 _DirtColor;
                float _HeightThreshold;
                float _FadeRange;
                
                float4 _GrassColor;
                float4 _GrassVariation;
                float _GrassHeight;
                float _GrassFade;
                float _GrassSlope;

                float _PixelSize;
                float _NoiseScale;
                float _NoiseIntensity;
                float _ShadowIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2; // Правильний спосіб передачі координат тіней в URP
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // Розрахунок координат тіней
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);
                #else
                    OUT.shadowCoord = float4(0, 0, 0, 0);
                #endif
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Отримання інтенсивності тіні
                half shadowAttenuation = 1.0;
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    Light mainLight = GetMainLight(IN.shadowCoord);
                    shadowAttenuation = mainLight.shadowAttenuation;
                #endif

                float yPos = IN.positionWS.y;
                float xPos = IN.positionWS.x;
                float zPos = IN.positionWS.z;
                
                float3 normalWS = normalize(IN.normalWS);

                if (_PixelSize > 0.0)
                {
                    yPos = floor(yPos / _PixelSize) * _PixelSize;
                    xPos = floor(xPos / _PixelSize) * _PixelSize;
                    zPos = floor(zPos / _PixelSize) * _PixelSize;
                }

                // 1. Процедурний шум
                float noise = sin(xPos * _NoiseScale) * cos(zPos * _NoiseScale);
                noise = noise * 0.5 + 0.5; 

                // 2. Земля
                float earthWeight = saturate((_HeightThreshold - yPos) / _FadeRange);
                half3 baseEarthColor = lerp(_TopColor.rgb, _BottomColor.rgb, earthWeight);
                half3 finalEarthColor = lerp(baseEarthColor, _DirtColor.rgb, noise * _NoiseIntensity);

                // 3. Трава
                half3 grassFinalColor = lerp(_GrassColor.rgb, _GrassVariation.rgb, noise * _NoiseIntensity);

                // 4. Маска трави
                float grassHeightMask = smoothstep(_GrassHeight - _GrassFade, _GrassHeight, yPos);
                float grassSlopeMask = smoothstep(_GrassSlope - 0.15, _GrassSlope + 0.05, normalWS.y);
                float totalGrassMask = grassHeightMask * grassSlopeMask;

                // 5. Альбедо (базовий колір без освітлення)
                half3 albedo = lerp(finalEarthColor, grassFinalColor, totalGrassMask);

                // 6. Застосування тіней (не дозволяємо тіні бути абсолютно чорною)
                half shadowVisual = lerp(1.0 - _ShadowIntensity, 1.0, shadowAttenuation);
                half3 finalColor = albedo * shadowVisual;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // =============================================
        // PASS 2 — Shadow Caster
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