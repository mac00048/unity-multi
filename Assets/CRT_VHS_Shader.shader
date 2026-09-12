Shader "Custom/CRT_VHS"
{
    Properties
    {
        _EffectStrength ("Effect Strength", Range(0, 1)) = 0.7

        _Curvature ("Curvature", Range(0, 0.2)) = 0.04
        _Scanlines ("Scanlines", Range(0, 1)) = 0.12
        _Noise ("VHS Noise", Range(0, 1)) = 0.04
        _Chromatic ("Chromatic Aberration", Range(0, 0.02)) = 0.001
        _Vignette ("Vignette", Range(0, 1)) = 0.20
        _Flicker ("Flicker", Range(0, 0.1)) = 0.005
        _Glitch ("Glitch", Range(0, 0.05)) = 0.002
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "CRT_VHS"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _EffectStrength;

            float _Curvature;
            float _Scanlines;
            float _Noise;
            float _Chromatic;
            float _Vignette;
            float _Flicker;
            float _Glitch;


            // =========================================================
            // STRUCTS
            // =========================================================

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };


            // =========================================================
            // RANDOM
            // =========================================================

            float GetRandom(float2 uv)
            {
                return frac(
                    sin(
                        dot(
                            uv,
                            float2(12.9898, 78.233)
                        )
                    ) * 43758.5453
                );
            }


            // =========================================================
            // CRT CURVATURE
            // =========================================================

            float2 ApplyCurvature(float2 uv)
            {
                float2 position =
                    uv * 2.0 - 1.0;

                float radius =
                    dot(position, position);

                position *=
                    1.0 +
                    radius * _Curvature;

                return position * 0.5 + 0.5;
            }


            // =========================================================
            // VERTEX
            // =========================================================

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv = float2(
                    (input.vertexID << 1) & 2,
                    input.vertexID & 2
                );

                output.texcoord = uv;

                output.positionCS =
                    float4(
                        uv * 2.0 - 1.0,
                        0.0,
                        1.0
                    );

                return output;
            }


            // =========================================================
            // FRAGMENT
            // =========================================================

            half4 Frag(Varyings input) : SV_Target
            {
                // Original screen
                half4 original =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        input.texcoord
                    );

                float2 uv =
                    input.texcoord;

                float time =
                    _Time.y;


                // =====================================================
                // CRT CURVATURE
                // =====================================================

                uv =
                    ApplyCurvature(uv);


                // =====================================================
                // VHS GLITCH
                // =====================================================

                float lineIndex =
                    floor(uv.y * 120.0);

                float glitchNoise =
                    GetRandom(
                        float2(
                            lineIndex,
                            floor(time * 8.0)
                        )
                    );

                float glitchAmount =
                    step(
                        0.94,
                        glitchNoise
                    ) * _Glitch;

                uv.x += glitchAmount;


                // =====================================================
                // SCREEN BOUNDS
                // =====================================================

                if (
                    uv.x < 0.0 ||
                    uv.x > 1.0 ||
                    uv.y < 0.0 ||
                    uv.y > 1.0
                )
                {
                    // Outside curved screen:
                    // keep original image
                    return original;
                }


                // =====================================================
                // CHROMATIC ABERRATION
                // =====================================================

                float2 chromaticOffset =
                    float2(
                        _Chromatic,
                        0.0
                    );

                half red =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        uv + chromaticOffset
                    ).r;

                half green =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        uv
                    ).g;

                half blue =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        uv - chromaticOffset
                    ).b;

                float3 color =
                    float3(
                        red,
                        green,
                        blue
                    );


                // =====================================================
                // SCANLINES
                // =====================================================

                float scanline =
                    sin(
                        uv.y *
                        _ScreenParams.y *
                        1.5
                    );

                scanline =
                    scanline * 0.5 + 0.5;

                color *=
                    lerp(
                        1.0,
                        scanline,
                        _Scanlines
                    );


                // =====================================================
                // VHS NOISE
                // =====================================================

                float noise =
                    GetRandom(
                        uv *
                        _ScreenParams.xy +
                        time * 100.0
                    );

                color +=
                    (noise - 0.5) *
                    _Noise;


                // =====================================================
                // VIGNETTE
                // =====================================================

                float2 centered =
                    uv * 2.0 - 1.0;

                float vignette =
                    1.0 -
                    dot(
                        centered,
                        centered
                    ) * _Vignette;

                color *=
                    saturate(vignette);


                // =====================================================
                // FLICKER
                // =====================================================

                float flicker =
                    1.0 +
                    sin(time * 40.0) *
                    _Flicker;

                color *=
                    flicker;


                // =====================================================
                // EFFECT STRENGTH
                // =====================================================

                float3 finalColor =
                    lerp(
                        original.rgb,
                        color,
                        _EffectStrength
                    );


                // =====================================================
                // OUTPUT
                // =====================================================

                return half4(
                    saturate(finalColor),
                    original.a
                );
            }

            ENDHLSL
        }
    }
}