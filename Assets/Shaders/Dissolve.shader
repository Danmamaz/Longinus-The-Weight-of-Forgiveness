Shader "Longinus/Dissolve"
{
    Properties
    {
        _BaseMap        ("Base Map", 2D)                 = "white" {}
        _BaseColor      ("Base Color", Color)            = (1, 1, 1, 1)
        [HDR] _EdgeColor("Edge Color", Color)            = (1, 0.267, 0, 1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth      ("Edge Width", Range(0, 0.3))    = 0.05
        _NoiseScale     ("Noise Scale", Float)           = 5
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "TransparentCutout"
            "Queue"          = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Shared declarations included in every pass
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _EdgeColor;
            float  _DissolveAmount;
            float  _EdgeWidth;
            float  _NoiseScale;
        CBUFFER_END

        // Value noise matching Unity Shader Graph's Simple Noise node
        float2 _Hash(float2 p)
        {
            p = float2(dot(p, float2(127.1f, 311.7f)),
                       dot(p, float2(269.5f, 183.3f)));
            return frac(sin(p) * 43758.5453f);
        }

        float SimpleNoise(float2 worldXZ)
        {
            float2 uv = worldXZ * _NoiseScale;
            float2 i  = floor(uv);
            float2 f  = frac(uv);
            float2 u  = f * f * (3.0f - 2.0f * f);

            return lerp(
                lerp(_Hash(i).x,                _Hash(i + float2(1, 0)).x, u.x),
                lerp(_Hash(i + float2(0, 1)).x, _Hash(i + float2(1, 1)).x, u.x),
                u.y);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
            };

            Varyings Vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float noise = SimpleNoise(IN.positionWS.xz);

                // Discard pixels below the dissolve threshold
                clip(noise - _DissolveAmount);

                // Bright band at the dissolve edge, falls off quickly above threshold
                float edgeStep   = smoothstep(_DissolveAmount,
                                              _DissolveAmount + max(_EdgeWidth, 0.001f),
                                              noise);
                float edgeFactor = 1.0f - edgeStep;

                float4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float4 albedo    = texSample * _BaseColor;

                Light mainLight = GetMainLight();
                float NdotL     = saturate(dot(normalize(IN.normalWS), mainLight.direction));
                float3 lit      = albedo.rgb * (mainLight.color * NdotL * 0.7f + 0.3f);

                // HDR edge emission — multiplied by 5 so it punches through bloom
                float3 emission = _EdgeColor.rgb * edgeFactor * 5.0f;

                return float4(lit + emission, 1.0f);
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

            HLSLPROGRAM
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing

            float3 _LightDirection;

            struct ShadowAttr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            ShadowVaryings ShadowVert(ShadowAttr IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                ShadowVaryings OUT;
                float3 posWS  = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(posWS, normWS, _LightDirection));
                OUT.positionWS = posWS;
                return OUT;
            }

            float4 ShadowFrag(ShadowVaryings IN) : SV_Target
            {
                // Mirror the same clip so dissolve holes appear in shadows too
                clip(SimpleNoise(IN.positionWS.xz) - _DissolveAmount);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
