Shader "Custom/WaterShader"
{
    Properties
    {
        [Header(Depth and Water Colour)]
        _DepthGradientShallow("Min Depth Colour", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Max Depth Colour", Color) = (0.1961, 0.5725, 1, 1)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1

        [Header(Flow Map Properties)]
        _FlowMap("Flow Map", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Float) = 0.01
        _FlowStrength ("Flow Strength", Float) = 0.0075
        _FlowSize ("Flow Map Size", Float) = 2

        [Header(Foam Properties)]
        _FoamTexture("Foam Texture", 2D) = "white" {}
        _FoamDistance("Foam Distance", Float) = 0.4
        _LightFoamColour("Light Foam Colour", Color) = (1, 1, 1, 1)
        _DarkFoamColour("Dark Foam Colour", Color) = (0.0314, 0.431, 0.690, 1)

        [Header(Voronoi Parameters)]
        _VoronoiScale("Voronoi Scale", Float) = 1.0
        _VoronoiSS("Voronoi Smoothing", Float) = 10.0
        _VoronoiLineThickness("Voronoi Line Thickness", Range(0.0, 1.0)) = 0.1
        [Toggle] _ProceduralVoronoi("Use Procedural Voronoi", Float) = 0
        [Toggle] _PreviewVoronoi("Preview Voronoi", Float) = 0

        [Header(Noise Properties)]
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseColour("Surface Noise Colour", Color) = (1,1,1,1)
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777

        [Header(Wave Properties)]
        _WaveChoppiness("Choppiness", Float) = 0.01
    }

    SubShader
    {
        Tags { 
            "RenderPipeline" = "UniversalRenderPipeline"
	        "Queue" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #define SMOOTHSTEP_AA 0.05

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            sampler2D _FlowMap;
            float4 _FlowMap_ST;
            float _FlowSpeed;
            float _FlowStrength;
            float _FlowSize;

            sampler2D _FoamTexture;
            float _FoamDistance;
            float4 _LightFoamColour;
            float4 _DarkFoamColour;

            float _VoronoiScale;
            float _VoronoiSS;
            float _VoronoiLineThickness;
            float _ProceduralVoronoi;
            float _PreviewVoronoi;

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            float4 _SurfaceNoiseColour;
            float _SurfaceNoiseCutoff;

            float _WaveChoppiness;

            // Procedural Voronoi noise function for foam texture
            // Hash functions for procedural noise
            float2 hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float Voronoi(float2 x)
            {
                float2 p = x;
                float2 ip = floor(p);
                float2 fp = frac(p);

                // First pass: Regular voronoi
                float2 mr = float2(0.0, 0.0);
                float2 mg = float2(0.0, 0.0);
                float md = 8.0;
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash2(ip + g);

                        float2 r = g + o - fp;
                        float d = dot(r, r);

                        if (d < md)
                        {
                            md = d;
                            mr = r;
                            mg = g;
                        }
                    }
                }

                // Second pass: Distance to borders
                md = 8.0;
                for (int j = -2; j <= 2; j++)
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        float2 g = mg + float2(i, j);
                        float2 o = hash2(ip + g);

                        float2 r = g + o - fp;

                        if (dot(mr - r, mr - r) > 0.00001)
                        {
                            float2 n = normalize(r - mr);
                            float dist = dot(0.5 * (mr + r), n);
                            md = min(md, dist);
                        }
                    }
                }

                return 1.0 - smoothstep(_VoronoiLineThickness - _VoronoiSS, _VoronoiLineThickness + _VoronoiSS, md);
            }

            // Alpha blend for final colour output
            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Get object space position
                float3 positionOS = IN.positionOS.xyz;

                // Transform to world space and sum the x and z components
                float3 positionWS = TransformObjectToWorld(positionOS);
                float xzSum = positionWS.x + positionWS.z;

                // Add time to create wave motion
                float waveInput = xzSum + _Time.y;

                // Compute sine wave for displacement and
                // multiply by choppiness to control amplitude
                float wave = sin(waveInput) + 0.5 * sin(waveInput * 2.3 + 1.7);
                float waveDisplacement = wave * _WaveChoppiness;

                // Apply displacement to y component of positionOS
                positionOS.y += waveDisplacement;

                // All for the depth texture
                OUT.positionHCS = TransformObjectToHClip(float4(positionOS, 1));
                OUT.screenPosition = ComputeScreenPos(OUT.positionHCS);
                OUT.uv = IN.uv;
                OUT.positionWS = positionWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // WATER COLOUR
                // Calculate water colour based on depth
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.screenPosition.xy / IN.screenPosition.w);
                float linearDepth = LinearEyeDepth(depth, _ZBufferParams);
                float depthDifference = linearDepth - IN.screenPosition.w;

                float waterDepthDifference = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColour = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference);

                // FLOW AND VORONOI
                // Distort UVs based on Flow Map
                float speedOverSizeTime = (_FlowSpeed / _FlowSize) * _Time.y;
                float2 flowMapUV = IN.uv + float2(speedOverSizeTime, speedOverSizeTime);

                // Sample the flow map and unpack normals
                // and calculate UV offset using flow strength
                float3 flowMapNormal = UnpackNormal(tex2D(_FlowMap, flowMapUV));
                float2 uvOffset = flowMapNormal.xy * _FlowStrength;

                // Sample the UV and the noise 
                float2 foamUV = (IN.uv + uvOffset) * _FlowSize;
                float2 foamPos = (IN.positionWS.xz * 1/_VoronoiScale + uvOffset * 5) * _FlowSize;
                float foamSample = tex2D(_FoamTexture, foamUV).r;

                if (_ProceduralVoronoi > 0.5) 
                {
                    foamSample = Voronoi(foamPos);
                }

                if (_PreviewVoronoi > 0.5)
                {
                    return float4(foamSample, foamSample, foamSample, 1.0);
                }

                // Sample the darker foam value
                // Lerp between water colour and dark foam colour
                float2 foamUVOffset = foamUV + float2(0.1, 0.1);
                float darkFoamSample = tex2D(_FoamTexture, foamUVOffset).r;

                if (_ProceduralVoronoi > 0.5) 
                {
                    darkFoamSample = Voronoi(foamPos + float2(0.1, 0.1));
                }

                float4 darkFoamColour = lerp(waterColour, _DarkFoamColour, darkFoamSample);

                // Lerp between the dark and light colour
                float4 finalWaterColour = lerp(darkFoamColour, _LightFoamColour, foamSample);

                // Sample surface noise
                float foamDepthDifference = saturate(depthDifference / _FoamDistance);
                float surfaceNoiseCutoff = foamDepthDifference * _SurfaceNoiseCutoff;

                // Animate the surface noise like the foam
                float surfaceNoiseSample = tex2D(_SurfaceNoise, foamUV).r;
                float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);
                float4 surfaceNoiseColour = _SurfaceNoiseColour;
                surfaceNoiseColour.a *= surfaceNoise;

                float4 outputColor = alphaBlend(surfaceNoiseColour, finalWaterColour);
                return outputColor;
            }
            ENDHLSL
        }
    }
}
