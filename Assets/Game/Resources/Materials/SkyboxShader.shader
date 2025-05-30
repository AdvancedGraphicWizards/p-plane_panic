// Unity built-in shader source. 
// Copyright (c) 2016 Unity Technologies. 
// MIT license (see license.txt)
// Modified by the Advanced Graphics Wizards

Shader "Custom/SkyboxShader" {
    Properties {
        [KeywordEnum(None, Simple, High Quality)] _SunDisk ("Sun", Int) = 2
        _SunSize ("Sun Size", Range(0,1)) = 0.04
        _SunSizeConvergence("Sun Size Convergence", Range(1,10)) = 5
        _AtmosphereThickness ("Atmosphere Thickness", Range(0,5)) = 1.0
        _Exposure("Exposure", Range(0, 8)) = 1.3

        // Adjust Rayleigh and Mie scattering
        _RayleighCoefficient ("Rayleigh Scattering Coefficient", Float) = 0.0025
        _MieCoefficient ("Mie Scattering Coefficient", Float) = 0.0010
        _ScatteringWavelength ("Scattering Wavelength", Vector) = (0.65, 0.57, 0.475, 1)
    }
    
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
    
        Pass {
    
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
    
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
    
            #pragma multi_compile_local _SUNDISK_NONE _SUNDISK_SIMPLE _SUNDISK_HIGH_QUALITY
    
            uniform half _Exposure;     // HDR exposure
            uniform half _SunSize;
            uniform half _SunSizeConvergence;
            uniform half _AtmosphereThickness;

            // Added variables for Rayleigh and Mie scattering from inspector
            uniform half _RayleighCoefficient;
            uniform half _MieCoefficient;
            uniform float3 _ScatteringWavelength;

            // Constants
            static const float kSunBrightness = 20.0; // Sun brightness
            static const float kMaxScatter = 50.0; // Maximum scattering value, to prevent math overflows on Adrenos
            static const float kOuterRadius = 1.025;
            static const float kOuterRadius2 = 1.0506;
            static const float kInnerRadius = 1.0;
            static const float kInnerRadius2 = 1.0;
            static const float kScaleDepth = 0.25;
            static const float kScale = 1.0 / (kOuterRadius - 1.0);
            static const float kSamples = 2.0; // THIS IS UNROLLED MANUALLY, DON'T TOUCH
            static const float kCameraHeight = 0.0001;
            static const float kScaleOverScaleDepth = (1.0 / (kOuterRadius - 1.0)) / 0.25;
    
            #if defined(UNITY_COLORSPACE_GAMMA)
                #define GAMMA 2
                #define COLOR_2_GAMMA(color) color
                #define COLOR_2_LINEAR(color) color*color
                #define LINEAR_2_OUTPUT(color) sqrt(color)
            #else
                #define GAMMA 2.2
                // HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
                #define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
                #define COLOR_2_LINEAR(color) color
                #define LINEAR_2_LINEAR(color) color
            #endif
    
            // RGB wavelengths
            // .35 (.62=158), .43 (.68=174), .525 (.75=190)
            static const float3 kDefaultScatteringWavelength = float3(.65, .57, .475);
            static const float3 kVariableRangeForScatteringWavelength = float3(.15, .15, .15);
    
            #define kRAYLEIGH (_RayleighCoefficient * pow(_AtmosphereThickness, 2.5)) // Rayleigh constant
            #define kMIE _MieCoefficient // Mie constant
    
            static const half kHDSundiskIntensityFactor = 15.0;
            static const half kSimpleSundiskIntensityFactor = 27.0;
            static const half kSunScale = 400.0 * kSunBrightness;
            static const float kKmESun = kMIE * kSunBrightness;
            static const float kKm4PI = kMIE * 4.0 * 3.14159265;
    
            #define MIE_G (-0.990)
            #define MIE_G2 0.9801
    
            // fine tuning of performance. You can override defines here if you want some specific setup
            // or keep as is and allow later code to set it according to target api
    
            // if set vprog will output color in final color space (instead of linear always)
            // in case of rendering in gamma mode that means that we will do lerps in gamma mode too, so there will be tiny difference around horizon
            // #define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
    
            // sun disk rendering:
            // no sun disk - the fastest option
            #define SKYBOX_SUNDISK_NONE 0
            // simplistic sun disk - without mie phase function
            #define SKYBOX_SUNDISK_SIMPLE 1
            // full calculation - uses mie phase function
            #define SKYBOX_SUNDISK_HQ 2
    
            // uncomment this line and change SKYBOX_SUNDISK_SIMPLE to override material settings
            // #define SKYBOX_SUNDISK SKYBOX_SUNDISK_SIMPLE
    
            #ifndef SKYBOX_SUNDISK
                #if defined(_SUNDISK_NONE)
                    #define SKYBOX_SUNDISK SKYBOX_SUNDISK_NONE
                #elif defined(_SUNDISK_SIMPLE)
                    #define SKYBOX_SUNDISK SKYBOX_SUNDISK_SIMPLE
                #else
                    #define SKYBOX_SUNDISK SKYBOX_SUNDISK_HQ
                #endif
            #endif
        
            #ifndef SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
                #if defined(SHADER_API_MOBILE)
                    #define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 1
                #else
                    #define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
                #endif
            #endif
    
            // Calculates the Rayleigh phase function
            half getRayleighPhase(half eyeCos2)
            {
                return saturate(0.75 + 0.75 * eyeCos2); // Clamp between 0 and 1.5
            }
            half getRayleighPhase(half3 light, half3 ray)
            {
                half eyeCos = dot(light, ray);
                return getRayleighPhase(eyeCos * eyeCos);
            }
    
    
            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
    
            struct v2f
            {
                float4  pos             : SV_POSITION;
    
            #if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
                // for HQ sun disk, we need vertex itself to calculate ray-dir per-pixel
                float3  vertex          : TEXCOORD0;
            #elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
                half3   rayDir          : TEXCOORD0;
            #else
                // as we dont need sun disk we need just rayDir.y (sky/ground threshold)
                half    skyGroundFactor : TEXCOORD0;
            #endif
    
                // calculate sky colors in vprog
                half3   groundColor     : TEXCOORD1;
                half3   skyColor        : TEXCOORD2;
    
            #if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
                half3   sunColor        : TEXCOORD3;
            #endif
    
                UNITY_VERTEX_OUTPUT_STEREO
            };
    
    
            float scale(float inCos)
            {
                float x = 1.0 - inCos;
                return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
            }
    
            v2f vert (appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.pos = UnityObjectToClipPos(v.vertex);
    
                // Adjust scattering wavelength
                float3 invWavelength = 1.0 / pow(_ScatteringWavelength, 4);

                // Updated calculations for Rayleigh and Mie coefficients
                float kKrESun = kRAYLEIGH * kSunBrightness;
                float kKr4PI = kRAYLEIGH * 4.0 * 3.14159265;
                float kKmESun = kMIE * kSunBrightness;
                float kKm4PI = kMIE * 4.0 * 3.14159265;
    
                float3 cameraPos = float3(0,kInnerRadius + kCameraHeight,0);    // The camera's current position
    
                // Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
                float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                eyeRay.y = max(eyeRay.y, 0.0);
    
                // Sky
                // Calculate the length of the "atmosphere"
                float far = sqrt(kOuterRadius2 + kInnerRadius2 * eyeRay.y * eyeRay.y - kInnerRadius2) - kInnerRadius * eyeRay.y;
                half3 cIn, cOut;
                float3 pos = cameraPos + far * eyeRay;

                // Calculate the ray's starting position, then calculate its scattering offset
                float height = kInnerRadius + kCameraHeight;
                float depth = exp(kScaleOverScaleDepth * (-kCameraHeight));
                float startAngle = dot(eyeRay, cameraPos) / height;
                float startOffset = depth*scale(startAngle);

                // Initialize the scattering loop variables
                float sampleLength = far / kSamples;
                float scaledLength = sampleLength * kScale;
                float3 sampleRay = eyeRay * sampleLength;
                float3 samplePoint = cameraPos + sampleRay * 0.5;

                // Now loop through the sample rays
                float3 frontColor = float3(0.0, 0.0, 0.0);
                // Weird workaround: WP8 and desktop FL_9_3 do not like the for loop here
                // (but an almost identical loop is perfectly fine in the ground calculations below)
                // Just unrolling this manually seems to make everything fine again.
                // for(int i=0; i<int(kSamples); i++)
                {
                    float height = length(samplePoint);
                    float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
                    float lightAngle = dot(_WorldSpaceLightPos0.xyz, samplePoint) / height;
                    float cameraAngle = dot(eyeRay, samplePoint) / height;
                    float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
                    float3 attenuate = exp(-clamp(scatter, 0.0, kMaxScatter) * (invWavelength * kKr4PI + kKm4PI));

                    frontColor += attenuate * (depth * scaledLength);
                    samplePoint += sampleRay;
                }
                {
                    float height = length(samplePoint);
                    float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
                    float lightAngle = dot(_WorldSpaceLightPos0.xyz, samplePoint) / height;
                    float cameraAngle = dot(eyeRay, samplePoint) / height;
                    float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
                    float3 attenuate = exp(-clamp(scatter, 0.0, kMaxScatter) * (invWavelength * kKr4PI + kKm4PI));

                    frontColor += attenuate * (depth * scaledLength);
                    samplePoint += sampleRay;
                }

                // Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
                cIn = frontColor * (invWavelength * kKrESun);
                cOut = frontColor * kKmESun;
    
                #if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
                    OUT.vertex          = -eyeRay;
                #elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
                    OUT.rayDir          = half3(-eyeRay);
                #else
                    OUT.skyGroundFactor = -eyeRay.y;
                #endif
        
                    // if we want to calculate color in vprog:
                    // 1. in case of linear: multiply by _Exposure in here (even in case of lerp it will be common multiplier, so we can skip mul in fshader)
                    // 2. in case of gamma and SKYBOX_COLOR_IN_TARGET_COLOR_SPACE: do sqrt right away instead of doing that in fshader
        
                    OUT.groundColor = _Exposure * (cIn * getRayleighPhase(_WorldSpaceLightPos0.xyz, -eyeRay));
                    OUT.skyColor = _Exposure * (cIn * getRayleighPhase(_WorldSpaceLightPos0.xyz, -eyeRay));
        
                #if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
                    // The sun should have a stable intensity in its course in the sky. Moreover it should match the highlight of a purely specular material.
                    // This matching was done using the standard shader BRDF1 on the 5/31/2017
                    // Finally we want the sun to be always bright even in LDR thus the normalization of the lightColor for low intensity.
                    half lightColorIntensity = clamp(length(_LightColor0.xyz), 0.25, 1);
                    #if SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
                        OUT.sunColor    = kSimpleSundiskIntensityFactor * saturate(cOut * kSunScale) * _LightColor0.xyz / lightColorIntensity;
                    #else // SKYBOX_SUNDISK_HQ
                        OUT.sunColor    = kHDSundiskIntensityFactor * saturate(cOut) * _LightColor0.xyz / lightColorIntensity;
                    #endif
        
                #endif
        
                #if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
                    OUT.groundColor = sqrt(OUT.skyColor);
                    OUT.skyColor    = sqrt(OUT.skyColor);
                    #if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
                        OUT.sunColor= sqrt(OUT.sunColor);
                    #endif
                #endif
    
                return OUT;
            }
    
    
            // Calculates the Mie phase function
            half getMiePhase(half eyeCos, half eyeCos2)
            {
                half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
                temp = pow(temp, pow(_SunSize,0.65) * 10);
                temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
                temp = 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
                #if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
                    temp = pow(temp, .454545);
                #endif
                return temp;
            }
    
            // Calculates the sun shape
            half calcSunAttenuation(half3 lightPos, half3 ray)
            {
            #if SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
                half3 delta = lightPos - ray;
                half dist = length(delta);
                half spot = 1.0 - smoothstep(0.0, _SunSize, dist);
                return spot * spot;
            #else // SKYBOX_SUNDISK_HQ
                half focusedEyeCos = pow(saturate(dot(lightPos, ray)), _SunSizeConvergence);
                return getMiePhase(-focusedEyeCos, focusedEyeCos * focusedEyeCos);
            #endif
            }
    
            half4 frag (v2f IN) : SV_Target
            { 
                half3 col = IN.skyColor;
    
                // if y > 1 [eyeRay.y < -SKY_GROUND_THRESHOLD] - ground
                // if y >= 0 and < 1 [eyeRay.y <= 0 and > -SKY_GROUND_THRESHOLD] - horizon
                // if y < 0 [eyeRay.y > 0] - sky
                #if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
                    half3 ray = normalize(IN.vertex.xyz);
                    half y = ray.y;
                #elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
                    half3 ray = IN.rayDir.xyz;
                    half y = ray.y;
                #else
                    half y = IN.skyGroundFactor;
                #endif

                // if we did precalculate color in vprog: just do lerp between them
                col = lerp(IN.skyColor, IN.skyColor, saturate(y));
    
                #if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
                    if(y < 0.0)
                    {
                        col += IN.sunColor * calcSunAttenuation(_WorldSpaceLightPos0.xyz, -ray);
                    }
                #endif
        
                #if defined(UNITY_COLORSPACE_GAMMA) && !SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
                    col = LINEAR_2_OUTPUT(col);
                #endif
    
                return half4(col,1.0);
    
            }
            ENDCG
        }
    }
    
    
    Fallback Off
    CustomEditor "SkyboxProceduralShaderGUI"
}