Shader "Longinus/FoliageBend"
{
    Properties
    {
        _BaseMap           ("Base Map", 2D)               = "white" {}
        _BaseColor         ("Base Color", Color)          = (1, 1, 1, 1)
        _AlphaCutoff       ("Alpha Cutoff", Range(0, 1))  = 0.4
        _WindStrength      ("Wind Strength", Float)       = 0.3
        _WindSpeed         ("Wind Speed", Float)          = 1.5
        // Exposed for inspector preview; overridden at runtime by GlobalFoliagePlayerSync
        _PlayerWorldPos    ("Player World Pos", Vector)   = (0, 0, 0, 0)
        _PlayerBendRadius  ("Player Bend Radius", Float)  = 3.0
        _PlayerBendStrength("Player Bend Strength", Float)= 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "TransparentCutout"
            "Queue"          = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float  _AlphaCutoff;
            float  _WindStrength;
            float  _WindSpeed;
            float  _PlayerBendRadius;
            float  _PlayerBendStrength;
        CBUFFER_END

        // Declared outside the per-material CBUFFER so Shader.SetGlobalVector updates
        // every foliage instance at once from GlobalFoliagePlayerSync.LateUpdate.
        float4 _PlayerWorldPos;

        // Returns XZ world-space displacement; Y is always 0 (no vertical stretch).
        // uvY = UV.y channel: 0 at roots (no bend), 1 at tips (full bend).
        float3 FoliageDisplacement(float3 worldPos, float uvY)
        {
            float windPhase   = _Time.y * _WindSpeed + worldPos.x * 0.7f + worldPos.z * 0.5f;
            float2 windOffset = float2(sin(windPhase), cos(windPhase * 0.6f)) * _WindStrength;

            float2 toPlayer   = worldPos.xz - _PlayerWorldPos.xz;
            float  dist       = length(toPlayer);
            float  pushFactor = saturate(1.0f - dist / max(_PlayerBendRadius, 0.001f));
            float2 pushDir    = dist > 0.001f ? (toPlayer / dist) : float2(0.0f, 1.0f);
            float2 pushOffset = pushDir * pushFactor * _PlayerBendStrength;

            float2 total = (windOffset + pushOffset) * uvY;
            return float3(total.x, 0.0f, total.y);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off    // double-sided — foliage cards must be lit from both sides

            HLSLPROGRAM
            #pragma vertex   FoliageVert
            #pragma fragment FoliageFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
            };

            Varyings FoliageVert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;

                float3 worldPos  = TransformObjectToWorld(IN.positionOS.xyz);
                worldPos        += FoliageDisplacement(worldPos, IN.uv.y);

                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            float4 FoliageFrag(Varyings IN) : SV_Target
            {
                float4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float4 albedo    = texSample * _BaseColor;
                clip(albedo.a - _AlphaCutoff);

                Light mainLight = GetMainLight();
                // abs(NdotL) avoids black back-faces on double-sided geometry
                float NdotL     = abs(dot(normalize(IN.normalWS), mainLight.direction));
                float3 lit      = albedo.rgb * (mainLight.color * NdotL * 0.8f + 0.2f);

                return float4(lit, 1.0f);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex   FoliageShadowVert
            #pragma fragment FoliageShadowFrag
            #pragma multi_compile_instancing

            float3 _LightDirection;

            struct ShadowAttr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            ShadowVaryings FoliageShadowVert(ShadowAttr IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                ShadowVaryings OUT;

                float3 worldPos  = TransformObjectToWorld(IN.positionOS.xyz);
                // Apply same displacement so shadows move with the foliage
                worldPos        += FoliageDisplacement(worldPos, IN.uv.y);

                float3 normWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS   = TransformWorldToHClip(
                    ApplyShadowBias(worldPos, normWS, _LightDirection));
                OUT.uv           = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 FoliageShadowFrag(ShadowVaryings IN) : SV_Target
            {
                float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a * _BaseColor.a;
                clip(alpha - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
