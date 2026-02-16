Shader "Unlit/PixelSandURP"
{
    Properties
    {
        _MainTex ("Texture Mask (Alpha)", 2D) = "white" {}
        _PixelSize ("World Pixel Size", Float) = 0.0625

        _DrySandColor ("Dry Sand Color", Color) = (0.76, 0.70, 0.50, 1)
        _WetDarken ("Wet Darken Multiplier", Range(0.1, 1.0)) = 0.55

        _WaterLevel ("Water Level (World Y)", Float) = 0.0
        _TideHeight ("Tide Height", Float) = 0.15
        _TideSpeed ("Tide Speed", Float) = 0.4
        _WetFadeRange ("Wet Fade Range", Float) = 0.2

        _FlowDir ("Flow Direction (XY)", Vector) = (1, 0.3, 0, 0)
        _FlowSpeed ("Flow Speed", Float) = 0.08
        _NoiseScale ("Noise World Scale", Float) = 6.0
        _NoiseScale2 ("Noise Layer 2 Scale", Float) = 4.2

        _GlitterColor ("Glitter Color", Color) = (1.0, 0.95, 0.8, 1)
        _GlitterPower ("Glitter Power", Float) = 64
        _GlitterIntensity ("Glitter Intensity", Range(0, 2)) = 0.8
        _FakeLightDir ("Fake Light Dir", Vector) = (0.4, 0.9, 0.3, 0)

        _GrainScale ("Grain Noise Scale", Float) = 28.0
        _GrainStrength ("Grain Strength", Range(0, 0.3)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 100

        // =============================================
        // PASS 1 — Main color pass
        // =============================================
        Pass
        {
            Name "SandPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _PixelSize;
                float4 _DrySandColor;
                float _WetDarken;
                float _WaterLevel;
                float _TideHeight;
                float _TideSpeed;
                float _WetFadeRange;
                float4 _FlowDir;
                float _FlowSpeed;
                float _NoiseScale;
                float _NoiseScale2;
                float4 _GlitterColor;
                float _GlitterPower;
                float _GlitterIntensity;
                float4 _FakeLightDir;
                float _GrainScale;
                float _GrainStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float2 voronoi(float2 uv)
            {
                float2 n = floor(uv);
                float2 f = frac(uv);
                float f1 = 8.0;
                float f2 = 8.0;

                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        float2 diff = g + o - f;
                        float d = dot(diff, diff);

                        if (d < f1)
                        {
                            f2 = f1;
                            f1 = d;
                        }
                        else if (d < f2)
                        {
                            f2 = d;
                        }
                    }
                }

                return float2(sqrt(f1), sqrt(f2));
            }

            float valueNoise(float2 uv)
            {
                float2 ig = floor(uv);
                float2 fg = frac(uv);
                float2 u = fg * fg * (3.0 - 2.0 * fg);

                float a = hash22(ig).x;
                float b = hash22(ig + float2(1.0, 0.0)).x;
                float c = hash22(ig + float2(0.0, 1.0)).x;
                float d = hash22(ig + float2(1.0, 1.0)).x;

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float2 snapWorldXZ(float3 posWS)
            {
                return floor(posWS.xz / _PixelSize) * _PixelSize;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                if (mask.a < 0.1)
                    discard;

                float2 snappedXZ = snapWorldXZ(IN.positionWS);

                float2 flowDir = normalize(_FlowDir.xy + float2(0.0001, 0.0001));
                float t = _Time.y * _FlowSpeed;

                float2 uvA = snappedXZ * _NoiseScale + flowDir * t;
                float2 vorA = voronoi(uvA);

                float2 uvB = snappedXZ * _NoiseScale2 + flowDir.yx * t * 1.3;
                float2 vorB = voronoi(uvB);

                float noiseA = saturate(vorA.y - vorA.x);
                float noiseB = saturate(vorB.y - vorB.x);
                float combinedNoise = saturate((noiseA + noiseB) * 0.5);
                combinedNoise = floor(combinedNoise * 8.0) / 8.0;

                float tideSine = sin(_Time.y * _TideSpeed) * 0.5 + 0.5;
                float noiseOffset = combinedNoise * _TideHeight;
                float effectiveWaterLevel = _WaterLevel + tideSine * _TideHeight + noiseOffset;

                float rawWetness = 1.0 - saturate((IN.positionWS.y - effectiveWaterLevel) / _WetFadeRange);
                float wetness = floor(rawWetness * 5.0 + 0.5) / 5.0;
                wetness = saturate(wetness);

                float grain = valueNoise(snappedXZ * _GrainScale);
                grain = floor(grain * 4.0) / 4.0;

                half3 drySand = _DrySandColor.rgb + (grain - 0.5) * _GrainStrength;
                half3 wetSand = drySand * _WetDarken;
                half3 sandColor = lerp(drySand, wetSand, wetness);

                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 L = normalize(_FakeLightDir.xyz);
                float3 H = normalize(L + V);
                float NdotH = max(dot(N, H), 0.0);

                float glitterMask = hash22(snappedXZ * 73.13).x;
                glitterMask = step(0.7, glitterMask);

                float spec = pow(NdotH, _GlitterPower) * _GlitterIntensity;
                spec = floor(spec * 4.0) / 4.0;

                half3 glitter = _GlitterColor.rgb * spec * glitterMask * wetness;
                sandColor += glitter;

                return half4(sandColor, mask.a);
            }

            ENDHLSL
        }

        // =============================================
        // PASS 2 — DepthOnly
        // Without this pass the sand mesh does NOT
        // appear in _CameraDepthTexture, so the water
        // shader's SampleSceneDepth() sees the far
        // plane and waterDepth is huge -> no foam.
        // =============================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _PixelSize;
                float4 _DrySandColor;
                float _WetDarken;
                float _WaterLevel;
                float _TideHeight;
                float _TideSpeed;
                float _WetFadeRange;
                float4 _FlowDir;
                float _FlowSpeed;
                float _NoiseScale;
                float _NoiseScale2;
                float4 _GlitterColor;
                float _GlitterPower;
                float _GlitterIntensity;
                float4 _FakeLightDir;
                float _GrainScale;
                float _GrainStrength;
            CBUFFER_END

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            DepthVaryings DepthVert(DepthAttributes IN)
            {
                DepthVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 DepthFrag(DepthVaryings IN) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                if (mask.a < 0.1)
                    discard;
                return 0;
            }

            ENDHLSL
        }

        // =============================================
        // PASS 3 — DepthNormals
        // Required by some URP features (SSAO, etc.)
        // and ensures depth is available in all modes.
        // =============================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _PixelSize;
                float4 _DrySandColor;
                float _WetDarken;
                float _WaterLevel;
                float _TideHeight;
                float _TideSpeed;
                float _WetFadeRange;
                float4 _FlowDir;
                float _FlowSpeed;
                float _NoiseScale;
                float _NoiseScale2;
                float4 _GlitterColor;
                float _GlitterPower;
                float _GlitterIntensity;
                float4 _FakeLightDir;
                float _GrainScale;
                float _GrainStrength;
            CBUFFER_END

            struct DNAttributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct DNVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            DNVaryings DepthNormalsVert(DNAttributes IN)
            {
                DNVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 DepthNormalsFrag(DNVaryings IN) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                if (mask.a < 0.1)
                    discard;

                float3 normalWS = normalize(IN.normalWS);
                return half4(normalWS * 0.5 + 0.5, 0.0);
            }

            ENDHLSL
        }
    }
}