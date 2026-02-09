Shader "Unlit/PixelPondURP"
{
    Properties
    {
        [Header(Base Settings)]
        _MainTex ("Texture Mask (Alpha)", 2D) = "white" {}
        _PixelSize ("Pixel Size (Snap)", Float) = 4.0
        
        [Header(Colors)]
        _ShallowColor ("Shallow Color", Color) = (0.68, 0.85, 0.9, 1)
        _DeepColor ("Deep Color", Color) = (0.0, 0.2, 0.4, 1)
        _FoamColor ("Foam Color", Color) = (0.88, 1, 1, 1)
        _HighlightColor ("Specular Highlight", Color) = (1, 1, 1, 1)
        
        [Header(Waves)]
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 20.0
        _WaveIntensity ("Wave Intensity", Range(0, 1)) = 0.1
        _NoiseScaleX ("Noise Stretch X", Float) = 2.0
        _NoiseScaleY ("Noise Stretch Y", Float) = 0.5
        
        [Header(Refraction)]
        _DistortionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02
        _FoamThreshold ("Foam Threshold (Units)", Float) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _PixelSize;
            float4 _ShallowColor;
            float4 _DeepColor;
            float4 _FoamColor;
            float4 _HighlightColor;
            
            float _WaveSpeed;
            float _WaveScale;
            float _WaveIntensity;
            float _NoiseScaleX;
            float _NoiseScaleY;
            float _DistortionStrength;
            float _FoamThreshold;

            float random (float2 uv) {
                return frac(sin(dot(uv.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            float noise (float2 uv) {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a)* u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 screenParams = _ScreenParams.xy;
                
                float2 snappedUV = floor(screenUV * screenParams / _PixelSize) / (screenParams / _PixelSize);
                
                half4 mask = tex2D(_MainTex, IN.uv);
                if(mask.a < 0.1) discard;

                float2 waveUV = snappedUV * _WaveScale;
                waveUV.x *= _NoiseScaleX;
                waveUV.y *= _NoiseScaleY;
                waveUV.x += _Time.y * _WaveSpeed; 
                
                float waveNoise = noise(waveUV);
                
                float rawDepth = SampleSceneDepth(snappedUV);
                float waterDepth = 0;

                if (unity_OrthoParams.w > 0.5)
                {
                    float sceneZ = rawDepth;
                    float surfaceZ = IN.positionCS.z; 

                    #if defined(UNITY_REVERSED_Z)
                        sceneZ = 1.0 - sceneZ;
                        surfaceZ = 1.0 - surfaceZ;
                    #endif

                    float orthoRange = _ProjectionParams.z - _ProjectionParams.y;
                    
                    sceneZ = sceneZ * orthoRange + _ProjectionParams.y;
                    surfaceZ = surfaceZ * orthoRange + _ProjectionParams.y;
                    
                    waterDepth = sceneZ - surfaceZ;
                }
                else
                {
                    float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float surfaceEyeDepth = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w, _ZBufferParams);
                    waterDepth = sceneEyeDepth - surfaceEyeDepth;
                }
                
                float2 distortionUV = snappedUV + (waveNoise * _DistortionStrength * mask.a);
                half3 background = SampleSceneColor(distortionUV);

                float depthBand = floor(waterDepth * 2.0) / 2.0; 
                half4 waterColor = lerp(_ShallowColor, _DeepColor, saturate(waterDepth + (waveNoise * _WaveIntensity)));
                
                if (waveNoise > 0.6) waterColor = lerp(waterColor, _DeepColor, 0.2);

                float foamLine = step(waterDepth, _FoamThreshold);
                half4 finalColor = lerp(waterColor, _FoamColor, foamLine);
                
                float highlight = step(0.85, waveNoise * (1.0 + _WaveIntensity));
                finalColor = lerp(finalColor, _HighlightColor, highlight);

                half3 outputRGB = lerp(background, finalColor.rgb, finalColor.a * 0.8);

                return half4(outputRGB, mask.a);
            }
            ENDHLSL
        }
    }
}