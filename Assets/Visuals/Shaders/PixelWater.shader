Shader "Unlit/PixelPondURP"
{
    Properties
    {
        _MainTex ("Texture Mask (Alpha)", 2D) = "white" {}
        _PixelSize ("World Pixel Size", Float) = 0.0625

        _ShallowColor ("Shallow Color", Color) = (0.18, 0.22, 0.20, 1)
        _DeepColor ("Deep Color", Color) = (0.02, 0.04, 0.05, 1)
        _FoamColor ("Foam / Shore Color", Color) = (0.35, 0.38, 0.30, 1)
        _HighlightColor ("Caustic Highlight", Color) = (0.45, 0.50, 0.42, 0.6)

        _AbsorptionCoeff ("Absorption Coefficient", Float) = 2.0
        _AbsorptionTint ("Absorption Tint (RGB)", Color) = (0.6, 0.75, 0.5, 1)

        _FlowDir ("Flow Direction (XY)", Vector) = (1, 0.3, 0, 0)
        _FlowSpeed ("Flow Speed", Float) = 0.08
        _NoiseScale ("Noise World Scale", Float) = 6.0
        _NoiseScale2 ("Noise Layer 2 Scale", Float) = 4.2
        _WaveIntensity ("Wave Intensity", Range(0, 1)) = 0.15

        _FoamDepthMax ("Foam Depth Max (units)", Float) = 0.35
        _FoamNoiseScale ("Foam Noise Scale", Float) = 12.0
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.45

        _DistortionStrength ("Refraction Strength", Range(0, 0.05)) = 0.008

        _HighlightThreshold ("Highlight Threshold", Range(0.5, 1.0)) = 0.82

        _DeepTransparency ("Deep Water Transparency", Range(0, 1)) = 0.3
        _DeepTransparencyFalloff ("Transparency Falloff Rate", Float) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "WaterPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _PixelSize;
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float4 _HighlightColor;
                float  _AbsorptionCoeff;
                float4 _AbsorptionTint;
                float4 _FlowDir;
                float  _FlowSpeed;
                float  _NoiseScale;
                float  _NoiseScale2;
                float  _WaveIntensity;
                float  _FoamDepthMax;
                float  _FoamNoiseScale;
                float  _FoamCutoff;
                float  _DistortionStrength;
                float  _HighlightThreshold;
                float  _DeepTransparency;
                float  _DeepTransparencyFalloff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 screenPos   : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            // Stable 2->2 hash (no sin, avoids GPU precision issues)
            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            // Voronoi distance field - returns (F1, F2)
            float2 voronoi(float2 uv)
            {
                float2 n = floor(uv);
                float2 f = frac(uv);

                float F1 = 8.0;
                float F2 = 8.0;

                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        float2 diff = g + o - f;
                        float  d = dot(diff, diff);

                        if (d < F1)
                        {
                            F2 = F1;
                            F1 = d;
                        }
                        else if (d < F2)
                        {
                            F2 = d;
                        }
                    }
                }

                return float2(sqrt(F1), sqrt(F2));
            }

            // Value noise (smooth, no sin-hash)
            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash22(i).x;
                float b = hash22(i + float2(1, 0)).x;
                float c = hash22(i + float2(0, 1)).x;
                float d = hash22(i + float2(1, 1)).x;

                return lerp(lerp(a, b, u.x),
                            lerp(c, d, u.x), u.y);
            }

            // World-space pixel snapping helper
            float2 snapWorldXZ(float3 posWS)
            {
                return floor(posWS.xz / _PixelSize) * _PixelSize;
            }

            // Robust orthographic depth (world-unit difference)
            float orthoDepthDifference(float rawDepthScene, float rawDepthSurface)
            {
                float near = _ProjectionParams.y;
                float far  = _ProjectionParams.z;

                float sceneZ   = rawDepthScene;
                float surfaceZ = rawDepthSurface;

                #if defined(UNITY_REVERSED_Z)
                    sceneZ   = 1.0 - sceneZ;
                    surfaceZ = 1.0 - surfaceZ;
                #endif

                float sceneEye   = lerp(near, far, sceneZ);
                float surfaceEye = lerp(near, far, surfaceZ);

                return sceneEye - surfaceEye;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS  = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos   = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // -- Mask --
                half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                if (mask.a < 0.1) discard;

                // -- Screen UV --
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // -- World-space snapped position --
                float2 snappedXZ = snapWorldXZ(IN.positionWS);

                // -- Dual-layer scrolling Voronoi --
                float2 flowDir = normalize(_FlowDir.xy + 0.0001);
                float  t       = _Time.y * _FlowSpeed;

                // Layer A: coarse, slow
                float2 uvA  = snappedXZ * _NoiseScale + flowDir * t;
                float2 vorA = voronoi(uvA);

                // Layer B: finer, slightly offset direction and faster
                float2 uvB  = snappedXZ * _NoiseScale2 + flowDir.yx * t * 1.3;
                float2 vorB = voronoi(uvB);

                // Blend: F2-F1 cell edge metric for a veiny liquid look
                float noiseA = saturate(vorA.y - vorA.x);
                float noiseB = saturate(vorB.y - vorB.x);
                float combinedNoise = saturate((noiseA + noiseB) * 0.5);

                // Pixel-snap the noise result
                combinedNoise = floor(combinedNoise * 8.0) / 8.0;

                // -- Depth --
                float rawDepth = SampleSceneDepth(screenUV);
                float surfaceZ = IN.positionCS.z;

                #if !defined(UNITY_REVERSED_Z)
                    surfaceZ = surfaceZ;
                #endif

                float waterDepth = 0.0;

                if (unity_OrthoParams.w > 0.5)
                {
                    waterDepth = orthoDepthDifference(rawDepth, surfaceZ);
                }
                else
                {
                    float sceneEye   = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float surfaceEye = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w, _ZBufferParams);
                    waterDepth = sceneEye - surfaceEye;
                }

                waterDepth = max(waterDepth, 0.0);

                // -- Beer's Law Absorption --
                float3 absorb = exp(-_AbsorptionCoeff * waterDepth * _AbsorptionTint.rgb);

                // Blend from shallow to deep using absorption
                half3 waterColor = _ShallowColor.rgb * absorb + _DeepColor.rgb * (1.0 - absorb);

                // Subtle wave modulation on top of absorption
                waterColor += combinedNoise * _WaveIntensity * _ShallowColor.rgb * absorb;

                // -- Foam / Shore Contact --
                float foamGradient = 1.0 - saturate(waterDepth / _FoamDepthMax);

                float foamNoise = valueNoise(snappedXZ * _FoamNoiseScale + flowDir * t * 0.5);
                foamNoise = floor(foamNoise * 6.0) / 6.0;

                float foam = step(_FoamCutoff, foamGradient * foamNoise + foamGradient * 0.5);
                waterColor = lerp(waterColor, _FoamColor.rgb, foam * _FoamColor.a);

                // -- Caustic Highlights --
                float highlight = step(_HighlightThreshold, combinedNoise) * (1.0 - foam);
                waterColor = lerp(waterColor, _HighlightColor.rgb, highlight * _HighlightColor.a);

                // -- Refraction --
                float2 distortUV  = screenUV + combinedNoise * _DistortionStrength;
                half3  background = SampleSceneColor(distortUV);

                // -- Final composite --
                // Absorption-based opacity: shallow water is more transparent
                float opacity = saturate(1.0 - exp(-_AbsorptionCoeff * waterDepth * 1.5));
                opacity = max(opacity, foam); // foam is always opaque

                // Deep water transparency:
                // Cap opacity so very deep water still lets some background through.
                // _DeepTransparency = 0 means fully opaque at depth (old behavior).
                // _DeepTransparency = 1 means fully transparent at depth.
                float deepFade = exp(-_DeepTransparencyFalloff * waterDepth);
                float maxOpacity = lerp(1.0, 1.0 - _DeepTransparency, 1.0 - deepFade);
                opacity = min(opacity, maxOpacity);

                opacity = max(opacity, 0.15); // safety floor so water is never invisible

                half3 outputRGB = lerp(background, waterColor, opacity);
                return half4(outputRGB, mask.a);
            }

            ENDHLSL
        }
    }
}