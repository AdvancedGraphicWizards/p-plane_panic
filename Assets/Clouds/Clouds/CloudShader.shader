Shader "Custom/CloudShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }
        LOD 100
        ZTest Always // skip z test
        ZWrite Off   
        Cull Off    

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "sdf.hlsl"

            struct VertexIn
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 posH : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                float4 pos: WORLD_POS;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            VertexOut vert(VertexIn vIn)
            {
                VertexOut vOut;
                vOut.posH = TransformObjectToHClip(vIn.positionOS.xyz);
                vOut.pos = vIn.positionOS;
                vOut.uv = TRANSFORM_TEX(vIn.uv, _MainTex);
                
                // Calculate the view vector
                float3 viewVector = mul(unity_CameraInvProjection, float4(vIn.uv * 2 - 1, 0, -1)).xyz;
                vOut.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0)).xyz;
                
                return vOut;
            }

            // Shape
            struct Shape {
                float3 origin;
                float pad1;
                float3 dim;
                float pad2;
            };
            StructuredBuffer<Shape> _ShapeBuffer;
            int _ShapeBufferLen;

            // Ray marching
            int _RaymarchMaxSteps;
            float _RaymarchHitDist;

            // Cloud Marching
            int _CloudMaxSteps;
            float _CloudStepSize;
            float _CloudRandomOffsetMultiplier;

            // Light marching
            int _LightMaxSteps;
            float _LightStepSize;
            float3 _LightDirection;
            float3 _LightPosition;

            // Cloud looks
            sampler3D _CloudNoiseTexture;
            float _CloudNoiseSampleMultiplier;
            float _CloudAbsorption;
            float3 _CloudScrollSpeed;
            float _CloudEdgeBlend;

            // Constants
            #define TRANSMITTANCE_CUTOFF 0.01
            #define LIGHT_COLOR float3(1.0, 1.0, 1.0)
            #define MAX_DISTANCE 1000.0
            #define EPS 0.01
            #define SHAPES 1

            float easeInCubic(float t) {
                return t * t * t;
            }

            // Util functions
            float sdf(float3 pos) 
            {
                float closestDist = FLT_MAX;
                for (int i = 0; i < SHAPES; i++) {
                    float dist = sdfBox(pos, _ShapeBuffer[i].origin, _ShapeBuffer[i].dim / 2);
                    closestDist = sdfUnion(closestDist, dist);
                }
                return closestDist;
            }
             
            float calculateAttenuation(float density, float distance) 
            {
                return exp(-_CloudAbsorption * density * distance);
            }

            float hash(float3 p)
            {
                const float3 primes = float3(127.1, 311.7, 74.7);
                float n = dot(p, primes);
                return frac(sin(n) * 43758.5453); 
            }

            float smoothstep(float edge0, float edge1, float x) {
                float t = saturate((x - edge0) / (edge1 - edge0));
                return t * t * (3.0 - 2.0 * t);
            }

            float sampleNoise(float3 pos) {
                float3 uv = pos * _CloudNoiseSampleMultiplier + _Time * _CloudScrollSpeed;
                float4 samp = tex3D(_CloudNoiseTexture, uv);
                float4 m = float4(6.0, 1.0, 1.0, 1.0); // how much noise to sample from each texture
                // float heightMult = smoothstep(1.0, 0.0, pos.y / 100);
                float dist = -sdf(pos);
                float heightMult = smoothstep(0.0, 1.0, dist / _CloudEdgeBlend);
                // float heightMult = exp(-pos.y / 70);
                return ((samp.r) * m.r + samp.g * m.g + samp.b * m.b + samp.a * m.a) / (m.r + m.g + m.b + m.a) * heightMult;
            }

            struct CloudData {          // size align
                float3 color;           // 3 4
                float transmittance;    // 4 4
                float dist;             // 5 8
                float distInCloud;      // 6 8
            };

            CloudData cloud(CloudData cloudData, float3 rayOrig, float3 rayDir, float depthDistance) {
                // apply random starting offset to avoid banding, if inside volume origin will be same, so take very small step in raydir before hashing
                float offset = hash(rayOrig + 0.1 * rayDir) * _CloudStepSize * _CloudRandomOffsetMultiplier;

                cloudData.distInCloud += offset;
                cloudData.dist += offset;

                // TODO huh not negative
                // float3 lightDir = _LightDirection;
                float3 p = rayOrig + rayDir * cloudData.dist;
                float3 lightDir = normalize(_LightPosition - p);
                [loop]
                for (int i = 0; i < _CloudMaxSteps; i++) {
                    // depth buffer check
                    if (cloudData.dist > depthDistance) {
                        break;
                    }

                    // exit bounds check
                    float3 pos = rayOrig + rayDir * cloudData.dist;
                    float dist = sdf(pos);
                    if (dist > _RaymarchHitDist + EPS) {  
                        break;
                    }

                    float stepSize = _CloudStepSize;

                    // light contribution
                    float density = sampleNoise(pos);
                    // float attenuation = calculateAttenuation(density, cloudData.distInCloud);
                    float attenuation = calculateAttenuation(density, stepSize);
                    cloudData.transmittance *= attenuation;
                    float3 color = LIGHT_COLOR * density;// * light(pos, lightDir);
                    cloudData.color += color * cloudData.transmittance * (1.0 - attenuation);

                    cloudData.dist += stepSize;
                    cloudData.distInCloud += stepSize;

                    if (cloudData.transmittance <= TRANSMITTANCE_CUTOFF) {
                        break;
                    }
                }

                return cloudData;
            }

            float4 frag(VertexOut vIn) : SV_Target
            {
                float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, vIn.uv).xyz;
                float2 uv = vIn.posH.xy / _ScaledScreenParams.xy;
                float3 cameraPos = _WorldSpaceCameraPos;

                // depth caluclations
                float depthBufferValue =  SampleSceneDepth(vIn.uv);
                float3 depthBufferWorlPos = ComputeWorldSpacePosition(uv, depthBufferValue, UNITY_MATRIX_I_VP);
                float depthDistance = length(cameraPos - depthBufferWorlPos);

                float3 rayOrig = cameraPos;
                float3 rayDir = normalize(vIn.viewVector);

                // Cloud data
                CloudData cloudData;
                cloudData.color = float3(0.0, 0.0, 0.0);
                cloudData.transmittance = 1.0;
                cloudData.dist = 0.0;
                cloudData.distInCloud = 0.0;

                [loop]
                for (int i = 0; i < _RaymarchMaxSteps; i++) {
                    // depth buffer check
                    bool hitObject = cloudData.dist > depthDistance;
                    bool farAway = cloudData.dist > 3000.0; // TODO cant use as variable
                    if (hitObject || farAway) {
                        break;
                    }

                    // check for inside cloud bounding box
                    float3 pos = rayOrig + rayDir * cloudData.dist;
                    float closestDist = sdf(pos);
                    if (closestDist < _RaymarchHitDist) {
                        cloudData = cloud(cloudData, rayOrig, rayDir, depthDistance); // calc acc and transmittance
                        if (cloudData.transmittance < TRANSMITTANCE_CUTOFF)  {
                            break;
                        }
                    } else {
                        cloudData.dist += closestDist;
                    }
                }

                if (cloudData.transmittance <= 0.0) {
                    cloudData.transmittance = 0.0;
                }

                float alpha = 1.0 - cloudData.transmittance;  // Convert transmittance to alpha
                float3 finalColor = lerp(cloudData.color, col, cloudData.transmittance);
                return float4(finalColor, 1.0);  // Use calculated alpha
                // float p = cloudData.transmittance;
                // // float cc = lerp(cloudData.color, col, p);
                // float3 finalColor = lerp(cloudData.color, col, p);
                // return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
