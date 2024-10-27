
//Adapted from https://github.com/MirzaBeig/Anime-Speed-Lines/issues/1

Shader "Custom/Screen-Space Speed-Lines"
{
    Properties
    {
        _MainTex("Screen", 2D) = "black" {}
        _Colour("Colour", Color) = (1, 1, 1, 1)
        _SpeedLinesTiling("Speed Lines Tiling", Float) = 200
        _SpeedLinesRadialScale("Speed Lines Radial Scale", Range(0, 10)) = 0.1
        _SpeedLinesPower("Speed Lines Power", Float) = 1
        _SpeedLinesRemap("Speed Lines Remap", Range(0, 1)) = 0.8
        _SpeedLinesAnimation("Speed Lines Animation", Float) = 3
        _MaskScale("Mask Scale", Range(0, 2)) = 1
        _MaskHardness("Mask Hardness", Range(0, 1)) = 0
        _MaskPower("Mask Power", Float) = 5
        [HideInInspector] _texcoord("", 2D) = "white" {}
        _SpeedLinesEnabled("Enable Speed Lines", Range(0, 1)) = 0
    }


    SubShader
    {
        LOD 0

        ZTest Always
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            
            struct appdata_img_custom
            {
                #if SHADER_API_GLES // if opengl
                float4 vertex : POSITION;
                half2 texcoord : TEXCOORD0;
                #else
                uint vertexID : SV_VertexID;
                #endif
            };

            struct v2f_img_custom
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half2 stereoUV : TEXCOORD2;

                #if UNITY_UV_STARTS_AT_TOP
                half4 uv2 : TEXCOORD1;
                half4 stereoUV2 : TEXCOORD3;
                #endif
            };

            sampler2D _MainTex;
            half4 _MainTex_TexelSize;
            half4 _MainTex_ST;

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float _SpeedLinesRadialScale;
            float _SpeedLinesTiling;
            float _SpeedLinesAnimation;
            float _SpeedLinesPower;
            float _SpeedLinesRemap;
            float _MaskScale;
            float _MaskHardness;
            float _MaskPower;
            float4 _Colour;
            float _SpeedLinesEnabled;

            // Noise function
            float3 mod2D289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float2 mod2D289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float3 permute(float3 x) { return mod2D289(((x * 34.0) + 1.0) * x); }
            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
                float2 i = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);
                float2 i1;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;
                i = mod2D289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
                float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
                m = m * m;
                m = m * m;
                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;
                m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
                float3 g;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }

            half4 ComputeGrabScreenPos(half2 uv, half4 screenPos)
            {
                return half4((screenPos.xy + uv * screenPos.zw), 0, 0);
            }

            v2f_img_custom vert(appdata_img_custom v)
            {
                v2f_img_custom output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
            #if SHADER_API_GLES
                float4 pos = v.vertex;
                float2 uv  = v.texcoord;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(v.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(v.vertexID);
            #endif

                output.pos = pos;
                output.uv = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;

                #if UNITY_UV_STARTS_AT_TOP
                output.uv2 = float4(pos.xy, 1, 1);
                output.stereoUV2 = ComputeGrabScreenPos(output.uv2, _MainTex_ST);

                if (_MainTex_TexelSize.y < 0.0)
                {
                    output.uv.y = 1.0 - output.uv.y;
                }
                #endif
                output.stereoUV = ComputeGrabScreenPos(output.uv, _MainTex_ST);

                return output;

            }


            half4 frag(v2f_img_custom i) : SV_Target
            {
                #ifdef UNITY_UV_STARTS_AT_TOP
                half2 uv = i.uv2;
                half2 stereoUV = i.stereoUV2;
                #else
                half2 uv = i.uv;
                half2 stereoUV = i.stereoUV;
                #endif

                half4 finalColor;

                // ase common template code
                float2 uv_MainTex = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 SceneColour7 = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv);

                // Check if the shader effect is enabled
                if (_SpeedLinesEnabled > 0)
                {
                    float2 CenteredUV15_g1 = (i.uv.xy - float2(0.5, 0.5));
                    float2 break17_g1 = CenteredUV15_g1;
                    float2 appendResult23_g1 = (float2((length(CenteredUV15_g1) * _SpeedLinesRadialScale * 2.0), (atan2(break17_g1.x, break17_g1.y) * (1.0 / 6.28318548202515) * _SpeedLinesTiling)));
                    float2 appendResult58 = (float2((-_SpeedLinesAnimation * _Time.y), 0.0));
                    float simplePerlin2D10 = snoise((appendResult23_g1 + appendResult58));
                    simplePerlin2D10 = simplePerlin2D10 * 0.5 + 0.5;
                    float temp_output_1_0_g6 = _SpeedLinesRemap;
                    float SpeedLines21 = saturate(((pow(simplePerlin2D10, _SpeedLinesPower) - temp_output_1_0_g6) / (1.0 - temp_output_1_0_g6)));
                    float2 texCoord60 = i.uv.xy * float2(2, 2) + float2(-1, -1);
                    float temp_output_1_0_g5 = _MaskScale;
                    float lerpResult71 = lerp(0.0, _MaskScale, _MaskHardness);
                    float Mask24 = pow((1.0 - saturate((length(texCoord60) - temp_output_1_0_g5) / ((lerpResult71 - 0.001) - temp_output_1_0_g5))), _MaskPower);
                    float MaskedSpeedLines29 = (SpeedLines21 * Mask24);
                    float ThresholdedSpeedLines = round(MaskedSpeedLines29);
                    float3 ColourRGB38 = (_Colour).rgb;
                    float ColourA40 = _Colour.a;
                    float4 lerpResult2 = lerp(SceneColour7, float4((MaskedSpeedLines29 * ColourRGB38), 0.0), (ThresholdedSpeedLines * ColourA40));

                    finalColor = lerpResult2;
                }
                else
                {
                    // If the effect is disabled, simply output the original color
                    finalColor = SceneColour7;
                }
                
                //return finalColor;
                return finalColor;
            }

            ENDHLSL
        }
    }
}