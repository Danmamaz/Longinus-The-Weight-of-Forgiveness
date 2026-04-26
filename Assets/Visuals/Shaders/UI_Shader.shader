Shader "Custom/OptimizedUIParticleFlow_Vectorized"
{
    Properties
    {
        _UnscaledTime ("Unscaled Time", Float) = 0
        [HideInInspector] _MainTex ("Texture (unused)", 2D) = "white" {}

        _Color           ("Background Color", Color)  = (0.02, 0.02, 0.05, 1)
        [HDR] _ParticleColor ("Particle Core Color", Color) = (0.5, 0.8, 2.0, 1)
        
        _GridDensity     ("Particle Density (Grid)", Range(10, 150)) = 50.0
        _Speed           ("Motion Speed", Range(0.1, 2.0)) = 0.5
        _FadeRange       ("Fade Out Distance", Range(0.01, 1.0)) = 0.1
        _GlowSpread      ("Glow Spread", Range(5.0, 30.0)) = 15.0
        
        _AngleSpread     ("Angle Spread", Range(0.0, 1.0)) = 0.4

        // UI Masking parameters
        _StencilComp     ("Stencil Comparison", Float) = 8
        _Stencil         ("Stencil ID",         Float) = 0
        _StencilOp       ("Stencil Operation",  Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask       ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha One // FIX: Changed to Additive Blending for proper light emission
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4    _Color;
            float4    _ParticleColor;
            float     _GridDensity;
            float     _Speed;
            float     _FadeRange;
            float     _GlowSpread;
            float     _AngleSpread;
            float4    _ClipRect;
            float _UnscaledTime;

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ParticleLayerVectorized(float2 uv, float density, float speed, float time, float fadeRange, float angleSpread, out float coreMask)
            {
                float2 st = uv * density;
                float2 rawId = floor(uv * density);
                float random = hash21(rawId);
                float flicker = sin(time * (3.0 + random * 5.0) + random * 100.0) * 0.4 + 0.6;
                float spawnMask = smoothstep(0.3, 0.8, random);

                float angle = (random - 0.5) * 2.0 * angleSpread;
                float2 moveDir = float2(cos(angle), sin(angle));
                float speedVar = 0.5 + random * 1.5;
                float2 startOffset = float2(hash21(rawId + 1.0), hash21(rawId + 2.0)) - 0.5;
                float t = time * speed * speedVar;
                float2 currentPos = frac(startOffset + moveDir * t) - 0.5;

                // FIX: Force the particle to fade out completely before it touches the cell boundary
                float edgeFade = smoothstep(0.5, 0.3, abs(currentPos.x)) * smoothstep(0.5, 0.3, abs(currentPos.y));

                float2 cellUV = frac(st) - 0.5;
                float d = length(cellUV - currentPos);

                float core = smoothstep(0.06, 0.0, d);
                float glow = exp(-d * _GlowSpread) * 0.5;
                float personalFadeEnd = fadeRange * (0.7 + 0.6 * hash21(rawId + 3.0));
                float personalFadeStart = personalFadeEnd * 0.5;
                float tailFade = 1.0 - smoothstep(personalFadeStart, personalFadeEnd, uv.x);
                float headFade = smoothstep(0.0, 0.02, uv.x);
                
                // FIX: Apply the edgeFade to the final visibility
                float finalVisibility = spawnMask * tailFade * headFade * flicker * edgeFade;

                coreMask = core * finalVisibility;
                return (core * 2.0 + glow) * finalVisibility;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.uv;
                o.color    = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (!UnityGet2DClipping(i.worldPos.xy, _ClipRect)) discard;
                float2 uv = i.uv;
                float time = _UnscaledTime;

                float c1, c2;
                float p1 = ParticleLayerVectorized(uv, _GridDensity, _Speed, time, _FadeRange, _AngleSpread, c1);
                float p2 = ParticleLayerVectorized(uv + float2(0.33, 0.77), _GridDensity * 1.4, _Speed * 0.7, time + 10.0, _FadeRange, _AngleSpread, c2);
                float totalParticles = p1 + p2;
                float totalCores = max(c1, c2);

                float3 finalRGB = _Color.rgb + (_ParticleColor.rgb * totalParticles);
                finalRGB += float3(1, 1, 1) * totalCores * 2.0; 

                float finalAlpha = saturate(_Color.a + totalParticles);

                return float4(finalRGB, finalAlpha) * i.color;
            }
            ENDCG
        }
    }
}